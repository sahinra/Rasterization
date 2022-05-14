﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rasterization
{
    class DrawPolygon : IDrawnShapes
    {
        public string Name { get; set; } = "Polygon";
        public Color Color { get; set; } = Colors.Black;
        public WriteableBitmap WriteableBitmap { get; set; }

        private List<Point> Points = new List<Point>();

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

        public void DeleteShape()
        {
            WriteableBitmap.Lock();
            try
            {
                ApplyModifiedDDA(Colors.Black);
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public void EditShape(int x, int y, int index)
        {
            double dx = x - Points[index].X;
            double dy = y - Points[index].Y;

            Point newPoint = new Point
            {
                X = Points[index].X + dx,
                Y = Points[index].Y + dy
            };

            DeleteShape();

            Points.RemoveAt(index);
            Points.Insert(index, newPoint);

            WriteableBitmap.Lock();
            try
            {
                ApplyModifiedDDA(Colors.Pink);
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public void Redraw(int x, int y)
        {
            double dx = x - Points[0].X;
            double dy = y - Points[0].Y;
            List<Point> newPoints = new List<Point>();

            foreach(Point p in Points)
            {
                Point newPoint = new Point
                {
                    X = p.X + dx,
                    Y = p.Y + dy
                };
                newPoints.Add(newPoint);
            }

            DeleteShape();
            Points.Clear();
            
            foreach (Point p in newPoints)
            {
                Points.Add(p);
            }

            WriteableBitmap.Lock();
            try
            {
                ApplyModifiedDDA(Colors.HotPink);
            }
            finally
            {
                WriteableBitmap.Unlock();
            }
        }

        public void MoveShape()
        {
            throw new NotImplementedException();
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

        public void ComputeDDAPoints(Point startPoint, Point endPoint, Color color)
        {
            Color = color;
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

            for (int i = 1; i <= step; i++)
            {
                SetPixel((int)startPoint.X, (int)startPoint.Y, color);
                SetPixel((int)endPoint.X, (int)endPoint.Y, color);

                startPoint.X += distanceX;
                startPoint.Y += distanceY;
            }
        }
    }
}
