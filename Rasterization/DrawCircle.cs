using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Rasterization
{
    class DrawCircle : IDrawnShapes
    {
        public string Name { get; set; } = "Circle";
        public Color Color { get; set; } = Colors.Red;
        public WriteableBitmap WriteableBitmap { get; set; }
        public int Radius { get; set; }

        public List<Point> Points = new List<Point>();

        public int Thickness { get; set; } = 1;

        public DrawCircle(Point center, Point endPoint)
        {
            Points.Add(center);
            Radius = MeasureDistance((int)center.X, (int)endPoint.X, (int)center.Y, (int)endPoint.Y);
        }

        public List<Point> GetPoints()
        {
            return Points;
        }

        int MeasureDistance(int x1, int x2, int y1, int y2)
        {
            return (int)Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        void SetPixel(int x, int y, Color color)
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


        public void midpoint(Color color)
        {
            Color = color;
            int R = Radius;
            int d = 1 - R;
            int x = Radius;
            int y = 0;
            //if(d < 0)
            //{
            //    x += 1;
            //    d += 2 * x + 1;
            //}
            //else
            //{
            //    x += 1;
            //    y -= 1;
            //    d += (2 * x) + 1 - (2 * y);
            //}


            while (y < x)
            {
                y++;
                if (d <= 0)
                {
                    d = d + 2 * y + 1;
                }
                else
                {
                    x--;
                    d = d + 2 * y - 2 * x + 1;
                }

                if (x < y)
                    break;

                Points.Add(new Point(x + Points[0].X, y + Points[0].Y));
                Points.Add(new Point(-x + Points[0].X, y + Points[0].Y));
                Points.Add(new Point(x + Points[0].X, -y + Points[0].Y));
                Points.Add(new Point(-x + Points[0].X, -y + Points[0].Y));

                if (x != y)
                {
                    Points.Add(new Point(y + Points[0].X, x + Points[0].Y));
                    Points.Add(new Point(-y + Points[0].X, x + Points[0].Y));
                    Points.Add(new Point(y + Points[0].X, -x + Points[0].Y));
                    Points.Add(new Point(-y + Points[0].X, -x + Points[0].Y));

                }
                if (Thickness <= 3) // 1, 2, 3
                {
                    SetPixel(x, y, color);
                    if (Thickness >= 2) // 2, 3
                    {
                        SetPixel(x + 1, y + 1, color);
                        if (Thickness == 3) // 3
                        {
                            SetPixel(x + 2, y + 2, color);
                        }
                    }
                }
            }
        }


        public void ApplyMidpointCircle(Color color)
        {
            Color = color;
            int R = Radius;
            int dE = 3;
            int dSE = 5 - (2 * R);
            int d = 1 - R;
            int x = 0;
            int y = R;

            SetPixel(x, y, Color);
            while (y > x)
            {
                if (d < 0) //move to E
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else //move to SE
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;
                if (Thickness <= 3) // 1, 2, 3
                {
                    SetPixel(x, y, color);
                    if (Thickness >= 2) // 2, 3
                    {
                        SetPixel(x + 1, y + 1, color);
                        if (Thickness == 3) // 3
                        {
                            SetPixel(x + 2, y + 2, color);
                        }
                    }
                }
            }
        }

        public void DeleteShape()
        {
            Draw(Colors.Black);
        }

        public void Draw(Color color)
        {
            WriteableBitmap.Lock();
            try
            {
                ApplyMidpointCircle(color);
                foreach (var point in Points)
                {
                    SetPixel((int)point.X, (int)point.Y, Color);

                }
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public void EditShape(int x, int y, int index)
        {
            int newRadius = MeasureDistance((int)Points[0].X, x, (int)Points[0].Y, y);
            Radius = newRadius;

            DeleteShape();

            Draw(Colors.Red);
        }

        public void MoveShape(int x, int y)
        {
            double dx = x - Points[0].X;
            double dy = y - Points[0].Y;

            Point newCenter = new Point
            {
                X = Points[0].X + dx,
                Y = Points[0].Y + dy
            };

            DeleteShape();

            Points.Clear();
            Points.Add(newCenter);

            Draw(Colors.Red);
        }
    }
}
