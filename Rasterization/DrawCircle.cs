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
            Points.Add(endPoint);
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

        public void ApplyMidpointCircle(Color color)
        {
            int R = Radius;
            int dE = 3;
            int dSE = 5 - (2 * R);
            int d = 1 - R;
            int x = 0;
            int y = R;

            SetPixel(x, y, Color);
            while (y > x)
            {
                if (d < 0)
                {
                    d += dE;
                    dE += 2;
                    dSE += 2;
                }
                else
                {
                    d += dSE;
                    dE += 2;
                    dSE += 4;
                    --y;
                }
                ++x;

                if (Thickness <= 3) // 1, 2, 3
                {
                    SetPixel((int)(x + Points[0].X), y + (int)Points[0].Y, color); //bottom
                    SetPixel((int)(-x + Points[0].X), y + (int)Points[0].Y, color); //bottom
                    SetPixel((int)(x + Points[0].X), -y + (int)Points[0].Y, color); //top
                    SetPixel((int)(-x + Points[0].X), -y + (int)Points[0].Y, color); //top

                    SetPixel((int)(y + Points[0].X), x + (int)Points[0].Y, color); //right
                    SetPixel((int)(y + Points[0].X), -x + (int)Points[0].Y, color); //right
                    SetPixel((int)(-y + Points[0].X), x + (int)Points[0].Y, color); //left              
                    SetPixel((int)(-y + Points[0].X), -x + (int)Points[0].Y, color); //left

                    if (Thickness >= 2) // 2, 3
                    {
                        SetPixel((int)(x + Points[0].X), y + (int)Points[0].Y + 1, color);
                        SetPixel((int)(-x + Points[0].X), y + (int)Points[0].Y + 1, color);
                        SetPixel((int)(x + Points[0].X), -y + (int)Points[0].Y - 1, color);
                        SetPixel((int)(-x + Points[0].X), -y + (int)Points[0].Y - 1, color);

                        SetPixel((int)(y + Points[0].X + 1), x + (int)Points[0].Y, color);
                        SetPixel((int)(y + Points[0].X + 1), -x + (int)Points[0].Y, color);
                        SetPixel((int)(-y + Points[0].X - 1), x + (int)Points[0].Y, color);                       
                        SetPixel((int)(-y + Points[0].X - 1), -x + (int)Points[0].Y, color);

                        if (Thickness == 3) // 3
                        {
                            SetPixel((int)(x + Points[0].X), y + (int)Points[0].Y + 2, color);
                            SetPixel((int)(-x + Points[0].X), y + (int)Points[0].Y + 2, color);
                            SetPixel((int)(x + Points[0].X), -y + (int)Points[0].Y - 2, color);
                            SetPixel((int)(-x + Points[0].X), -y + (int)Points[0].Y - 2, color);

                            SetPixel((int)(y + Points[0].X) + 2, x + (int)Points[0].Y, color);
                            SetPixel((int)(y + Points[0].X) + 2, -x + (int)Points[0].Y, color);
                            SetPixel((int)(-y + Points[0].X) - 2, x + (int)Points[0].Y, color);                           
                            SetPixel((int)(-y + Points[0].X) - 2, -x + (int)Points[0].Y, color);
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
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public void EditShape(int x, int y, int index)
        {
            DeleteShape();
            int newRadius = MeasureDistance((int)Points[0].X, x, (int)Points[0].Y, y);
            Radius = newRadius;
            
            Draw(Color);
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

            Points.RemoveAt(0);
            Points.Insert(0, newCenter);

            Draw(Color);
        }
    }
}
