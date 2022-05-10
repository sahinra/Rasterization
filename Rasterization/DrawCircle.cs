using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rasterization
{
    class DrawCircle : IDrawnShapes
    {
        public string Name { get; set; } = "Circle";
        public Color Color { get; set; } = Colors.Black;
        public WriteableBitmap WriteableBitmap { get; set; }
        public Point Center { get; set; }
        public int Radius { get; set; }

        public DrawCircle(Point center, Point endPoint)
        {
            Center = center;
            Radius = MeasureDistance((int)center.X, (int)center.Y, (int)endPoint.X, (int)endPoint.Y);         
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
                    y--;
                }
                ++x;
                SetPixel(x, y, color);
            }
        }

        public void DeleteShape()
        {
            WriteableBitmap.Lock();
            try
            {
                ApplyMidpointCircle(Colors.Black);
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
