using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rasterization
{
    public class DrawPolygon : IDrawnShapes
    {
        public string Name { get; set; } = "Polygon";
        public Color Color { get; set; } = Colors.Red;
        public Color FilledColor { get; set; } = Colors.Blue;
        public System.Drawing.Bitmap FilledImage { get; set; }
        public bool IsFilledImage { get; set; } = false;
        public bool IsFilledColor { get; set; } = false;
        public WriteableBitmap WriteableBitmap { get; set; }
        public int Thickness { get; set; } = 1;

        private List<Point> Points = new List<Point>();


        public struct EdgeTable //active edge table
        {
            public int y; //y max, y += 1
            public double x; //x of y min, x = x + 1/m
            public double slope; // 
        }

        public DrawPolygon(List<Point> points)
        {
            foreach(Point p in points)
            {
                Points.Add(p);
            }
        }

        public List<Point> GetPoints()
        {
            return Points;
        }

        public void ApplyModifiedDDA(Color color)
        {
            foreach(var p in Points)
            {
                int index = Points.IndexOf(p);
                if (index == Points.Count - 1)
                {
                    ComputeDDAPoints(Points[0], p, color);
                    return;
                }
                ComputeDDAPoints(p, Points[index + 1], color);
            }
        }

        public void FillPolygon(Color color)
        {
            int N = Points.Count();          
            List<EdgeTable> AET = new List<EdgeTable>();
            int[] indices = new int[N];

            var P = new List<Point>();

            foreach (var p in Points)
            {
                P.Add(new Point((int)p.X, (int)p.Y));
            }
            var P_Y = P.OrderBy(p => p.Y).ToList();
            
            for (int j = 0; j < N; j++)
                indices[j] = P.IndexOf(P.Find(p => p == P_Y[j]));

            int k = 0;
            int i = indices[k];
            int y, ymin, ymax;
            y = ymin = (int)P[indices[0]].Y;
            ymax = (int)P[indices[N - 1]].Y;

            while (y < ymax)
            {
                while (P[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (P[i - 1].Y > P[i].Y)
                        {
                            var aet_y = P[i - 1].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[i - 1].X - P[i].X) / (P[i - 1].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    else
                    {
                        if (P[N - 1].Y > P[i].Y)
                        {
                            var aet_y = P[N - 1].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[N - 1].X - P[i].X) / (P[N - 1].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    if (i < N - 1)
                    {
                        if (P[i + 1].Y > P[i].Y)
                        {
                            var aet_y = P[i + 1].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[i + 1].X - P[i].X) / (P[i + 1].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    else
                    {
                        if (P[0].Y > P[i].Y)
                        {
                            var aet_y = P[0].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[0].X - P[i].X) / (P[0].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    ++k;
                    i = indices[k];
                }
                //sort AET by x value
                AET = AET.OrderBy(aet => aet.x).ToList();
                Debug.WriteLine("AET count: " + AET.Count());

                WriteableBitmap.Lock();
                try
                {
                    for (int j = 0; j < AET.Count; j += 2)
                    {
                        if (j + 1 < AET.Count)
                        {
                            for (int x = (int)AET[j].x; x <= (int)AET[j + 1].x; x++)
                            {
                                SetPixel((int)x, (int)y, color);
                            }
                        }
                    }
                    ++y;
                }
                finally
                {
                    WriteableBitmap.Unlock();
                }

                //remove from AET edges for which ymax = y
                AET.RemoveAll(x => x.y == y);

                for (int j = 0; j < AET.Count; j++)
                {
                    EdgeTable newEdge = new EdgeTable();
                    newEdge.y = AET[j].y;
                    newEdge.x = AET[j].x + AET[j].slope;
                    newEdge.slope = AET[j].slope;
                    AET[j] = newEdge;
                }
            }

            if(color != Colors.Black)
            {
                IsFilledColor = true;
            }
            IsFilledImage = false;
        }

        public void DeleteShape()
        {
            Draw(Colors.Black);

            if (IsFilledColor || IsFilledImage)
            {
                FillPolygon(Colors.Black);
                IsFilledImage = false;
                IsFilledColor = false;
            }
        }

        public void Draw(Color color)
        {
            ApplyModifiedDDA(color);
        }

        public void EditShape(int x, int y, int index)
        {
            double dx = x - Points[index].X;
            double dy = y - Points[index].Y;
            int status = 0; // 1-filled color, 2-filled image

            Point newPoint = new Point
            {
                X = Points[index].X + dx,
                Y = Points[index].Y + dy
            };

            if (IsFilledColor)
            {
                status = 1;
            }
            if (IsFilledImage)
            {
                status = 2;
            }

            DeleteShape();

            Points.RemoveAt(index);
            Points.Insert(index, newPoint);

            Draw(Color);

            if (status == 2)
            {
                FillWithImage();
            }
            if (status == 1)
            {
                FillPolygon(FilledColor);
            }
            status = 0;
        }

        public void MoveShape(int x, int y)
        {
            double dx = x - Points[0].X;
            double dy = y - Points[0].Y;
            List<Point> newPoints = new List<Point>();
            int status = 0; // 1-filled color, 2-filled image

            foreach(Point p in Points)
            {
                Point newPoint = new Point
                {
                    X = p.X + dx, 
                    Y = p.Y + dy
                };
                newPoints.Add(newPoint);
            }

            if(IsFilledColor)
            {
                status = 1;
            }
            if(IsFilledImage)
            {
                status = 2;
            }

            DeleteShape();
            Points.Clear();
            
            foreach (Point p in newPoints)
            {
                Points.Add(p);
            }

            Draw(Color);

            if (status == 2)
            {
                FillWithImage();
            }
            if (status == 1)
            {
                FillPolygon(FilledColor);
            }
            status = 0;
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (y > WriteableBitmap.PixelHeight - 1 || x > WriteableBitmap.PixelWidth - 1)
                return;
            if (y < 0 || x < 0)
                return;

            IntPtr pBackBuffer = WriteableBitmap.BackBuffer;
            int stride = WriteableBitmap.BackBufferStride;

            unsafe
            {
                byte* pBuffer = (byte*)pBackBuffer.ToPointer();
                int location = y * stride + x * 4;

                pBuffer[location] = color.B;
                pBuffer[location + 1] = color.G;
                pBuffer[location + 2] = color.R;
                pBuffer[location + 3] = color.A;

            }
            WriteableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        }

        public void ComputeDDAPoints(Point startPoint, Point endPoint, Color color)
        {
            double distanceX = endPoint.X - startPoint.X;
            double distanceY = endPoint.Y - startPoint.Y;
            double step;

            if (Math.Abs(distanceX) > Math.Abs(distanceY))
            {
                step = Math.Abs(distanceX);
            }
            else
            {
                step = Math.Abs(distanceY);
            }

            distanceX = distanceX / step;
            distanceY = distanceY / step;

            WriteableBitmap.Lock();
            try
            {
                for (int i = 1; i <= step; i++)
                {
                    if (Thickness <= 3) // 1, 2, 3
                    {
                        SetPixel((int)startPoint.X, (int)startPoint.Y, color);
                        SetPixel((int)endPoint.X, (int)endPoint.Y, color);
                        if (Thickness >= 2) // 2, 3
                        {
                            SetPixel((int)startPoint.X + 1, (int)startPoint.Y + 1, color);
                            SetPixel((int)endPoint.X + 1, (int)endPoint.Y + 1, color);
                            if (Thickness == 3) // 3
                            {
                                SetPixel((int)startPoint.X + 2, (int)startPoint.Y + 2, color);
                                SetPixel((int)endPoint.X + 2, (int)endPoint.Y + 2, color);
                            }
                        }
                    }

                    startPoint.X += distanceX;
                    startPoint.Y += distanceY;
                }
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public static System.Drawing.Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public void FillWithImage()
        {
            bool IsResized = false;
            if (FilledImage == null)
                return;
            System.Drawing.Bitmap newBitmap = null;

            if (FilledImage.Width > 100 || FilledImage.Height > 100)
            {
                newBitmap = ResizeImage(FilledImage, 1000, 500);
                IsResized = true;
            }

            int N = Points.Count();
            List<EdgeTable> AET = new List<EdgeTable>();
            int[] indices = new int[N];

            var P = new List<Point>();

            foreach (var p in Points)
            {
                P.Add(new Point((int)p.X, (int)p.Y));
            }

            var P_Y = P.OrderBy(p => p.Y).ToList();

            for (int j = 0; j < N; j++)
                indices[j] = P.IndexOf(P.Find(p => p == P_Y[j]));

            int k = 0;
            int i = indices[k];
            double y, ymin, ymax;
            y = ymin = P[indices[0]].Y;
            ymax = P[indices[N - 1]].Y;

            while (y < ymax)
            {
                while (P[i].Y == y)
                {
                    if (i > 0)
                    {
                        if (P[i - 1].Y > P[i].Y)
                        {
                            var aet_y = P[i - 1].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[i - 1].X - P[i].X) / (P[i - 1].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    else
                    {
                        if (P[N - 1].Y > P[i].Y)
                        {
                            var aet_y = P[N - 1].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[N - 1].X - P[i].X) / (P[N - 1].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    if (i < N - 1)
                    {
                        if (P[i + 1].Y > P[i].Y)
                        {
                            var aet_y = P[i + 1].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[i + 1].X - P[i].X) / (P[i + 1].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    else
                    {
                        if (P[0].Y > P[i].Y)
                        {
                            var aet_y = P[0].Y;
                            var aet_xYmin = P[i].X;

                            EdgeTable newEdge = new EdgeTable();
                            newEdge.y = (int)aet_y;
                            newEdge.x = aet_xYmin;
                            newEdge.slope = (double)(P[0].X - P[i].X) / (P[0].Y - P[i].Y);
                            AET.Add(newEdge);
                        }
                    }
                    ++k;
                    i = indices[k];
                }
                AET = AET.OrderBy(aet => aet.x).ToList();

                WriteableBitmap.Lock();
                try
                {
                    for (int j = 0; j < AET.Count; j += 2)
                    {
                        if (j + 1 < AET.Count)
                        {
                            for (int x = (int)AET[j].x; x <= (int)AET[j + 1].x; x++)
                            {
                                if(IsResized)
                                {

                                    if (x > 0 && x < 1000 && y > 0 && y < 500)
                                    {
                                        SetPixel((int)x, (int)y, Color.FromArgb(newBitmap.GetPixel((int)x, (int)y).A, newBitmap.GetPixel((int)x,
                                                (int)y).R, newBitmap.GetPixel((int)x, (int)y).G, newBitmap.GetPixel((int)x, (int)y).B));
                                    }
                                }
                                else
                                {
                                    if (x > 0 && x < 1000 && y > 0 && y < 500)
                                    {
                                        SetPixel((int)x, (int)y, Color.FromArgb(FilledImage.GetPixel((int)x %FilledImage.Size.Width, (int)y % FilledImage.Size.Height).A, 
                                            FilledImage.GetPixel((int)x%FilledImage.Size.Width, (int)y % FilledImage.Size.Height).R, 
                                                FilledImage.GetPixel((int)x%FilledImage.Size.Width, (int)y % FilledImage.Size.Height).G, 
                                                    FilledImage.GetPixel((int)x % FilledImage.Size.Width, (int)y % FilledImage.Size.Height).B));
                                    }
                                }
                            }
                        }
                    }
                    ++y;
                }
                finally
                {
                    WriteableBitmap.Unlock();
                }

                AET.RemoveAll(x => x.y == y);

                for (int j = 0; j < AET.Count; j++)
                {
                    EdgeTable newEdge = new EdgeTable();
                    newEdge.y = AET[j].y;
                    newEdge.x = AET[j].x + AET[j].slope;
                    newEdge.slope = AET[j].slope;
                    AET[j] = newEdge;
                }
            }
            IsFilledImage = true;
            IsFilledColor = false;
        }

        public bool Clip(float denom, float numer, ref float tE, ref float tL)
        {
            if (denom == 0)
            {
                if (numer > 0)
                    return false;
                return true;
            }
            float t = numer / denom;
            if (denom > 0)
            {
                if (t > tL)
                    return false;
                if (t > tE)
                    tE = t;
            }
            else
            {
                if (t < tE)
                    return false;
                if (t < tL)
                    tL = t;
            }
            return true;
        }


        public void LiangBarsky(Point p1, Point p2, DrawRectangle clip)
        {
            float dx = (float)(p2.X - p1.X);
            float dy = (float)(p2.Y - p1.Y);
            float tE = 0;
            float tL = 1;
            if (Clip(-dx, (float)(p1.X - clip.LeftTop.X), ref tE, ref tL))
            {
                if (Clip(dx, (float)(clip.RightBottom.X - p1.X), ref tE, ref tL))
                {
                    if (Clip(-dy, (float)(p1.Y - clip.RightBottom.Y), ref tE, ref tL))
                    {
                        if (Clip(dy, (float)(clip.LeftTop.Y - p1.Y), ref tE, ref tL))
                        {
                            if (tL < 1)
                            {
                                p2.X = p1.X + dx * tL;
                                p2.Y = p1.Y + dy * tL;
                            }
                            if (tE > 0)
                            {
                                p1.X += dx * tE;
                                p1.Y += dy * tE;
                            }

                            WriteableBitmap.Lock();
                            try
                            {
                                ComputeDDAPoints(p1, p2, Colors.Blue);
                            }
                            finally
                            {
                                WriteableBitmap.Unlock();
                            }
                            //Draw(Colors.Blue);
                        }
                    }
                }
            }
        }

        public void ApplyLiangBarsky(DrawRectangle rect)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == Points.Count - 1)
                {
                    LiangBarsky(Points[i], Points[0], rect);
                }
                else
                {
                    LiangBarsky(Points[i], Points[i + 1], rect);
                }
            }
        }
    }
}
