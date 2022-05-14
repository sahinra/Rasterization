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
        public Color Color { get; set; } = Colors.Black;
        public WriteableBitmap WriteableBitmap { get; set; }
        public Point Center { get; set; }
        public int Radius { get; set; }
        public List<Point> Points = new List<Point>();

        public List<Point> GetPoints()
        {
            Points.Add(Center);
            return Points;
        }

        public DrawCircle(Point center, Point endPoint)
        {
            Center = center;
            Radius = MeasureDistance((int)center.X, (int)endPoint.X, (int)center.Y, (int)endPoint.Y);         
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
            Color = color;
            int R = Radius;
            int dE = 3;
            int num = 5;
            int r2 = 2 * R;
            int r3 = 1;
            int r4 = 2 * R;
            int dSE = num - r2;
            int d = r3 - r4;
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
                SetPixel(x, y, color);
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
            int newRadius = MeasureDistance((int)Center.X, x, (int)Center.Y, y);
            Radius = newRadius;

            DeleteShape();

            Draw(Colors.Pink);
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

            Draw(Colors.Violet);
        }
    }
}
