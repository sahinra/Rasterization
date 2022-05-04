using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Windows;
using System.Drawing;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace Rasterization
{
    class DDA
    {
        private static void ApplyDDA(Point startPoint, Point endPoint)
        {
            double distanceX = endPoint.X - startPoint.X;
            double distanceY = endPoint.Y - startPoint.Y;
            double step;
            double slope = (endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X);

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

                //startPoint.X += distanceX;
                //startPoint.Y += distanceY;
            }

        //    if (startPoint.Y < endPoint.Y)
        //    {
        //        if(startPoint.X < endPoint.X)
        //        {
        //            for (int i = (int)startPoint.X; i < distanceX; i++)
        //            {

        //            }
        //        }
        //        else
        //        {

        //        }
        //    }
        //    else
        //    {
        //        if (startPoint.X < endPoint.X)
        //        {
        //            for (int i = (int)startPoint.X; i < distanceX; i++)
        //            {

        //            }
        //        }
        //        else
        //        {

        //        }
        //    }
        }

        private void DrawLine(Point p1, Point p2)
        {
            Line line = new Line();
            line.X1 = p1.X;
            line.Y1 = p1.Y;
            line.X2 = p2.X;
            line.Y2 = p2.Y;
        }
    }
}
