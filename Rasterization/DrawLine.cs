using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Rasterization
{
    class DrawLine : IDrawnShapes
    {
        public string Name { get; set; } = "Line";
        public Color Color { get; set; } = Colors.Black;
        public WriteableBitmap WriteableBitmap { get; set; }
        public Point StartPoint {get; set;}
        public Point EndPoint { get; set; }
        public List<Point> Points { get; set; }

        public DrawLine(Point startPoint, Point endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public List<Point> GetPoints()
        {
            Points.Add(StartPoint);
            Points.Add(EndPoint);
            return Points;
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

        public void ApplyDDA(Color color)
        {
            Point startPoint = StartPoint;
            Point endPoint = EndPoint;
            Color = color;
            double distanceX = endPoint.X - startPoint.X;
            double distanceY = endPoint.Y - startPoint.Y;
            double step;

            //https://en.wikipedia.org/wiki/Digital_differential_analyzer_(graphics_algorithm)
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

            for (int i = 1; i <= step; i++)
            {
                SetPixel((int)startPoint.X, (int)startPoint.Y, color);
                SetPixel((int)endPoint.X, (int)endPoint.Y, color);

                startPoint.X += distanceX;
                startPoint.Y += distanceY;
            }
        }

        public void DeleteShape()
        {
            WriteableBitmap.Lock();
            try
            {
                SetPixel((int)StartPoint.X,(int) StartPoint.Y, Colors.Black);
                SetPixel((int)EndPoint.X, (int)EndPoint.Y, Colors.Black);
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public void EditShape()
        {
            throw new NotImplementedException();
        }

        public void MoveShape()
        {
            throw new NotImplementedException();
        }
    }
}
