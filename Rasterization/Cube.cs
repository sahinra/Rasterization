using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Rasterization
{
    public class CubePoint
    {
        public int a;
        public int b;
        public int c;
        public int e;
    }

    public class Cube
    {
        List<CubePoint> Points = new List<CubePoint>();

        int a = 1, b = 1, c = 1;
        int d = 4;
        double w = SystemParameters.WorkArea.Width;
        double h = SystemParameters.WorkArea.Height;

        public Cube()
        {
            GetPoints();
            GetMatrix();

        }

        private void GetPoints()
        {
            Points.Add(new CubePoint { a = -a, b = 0, c = c, e = 1 });
            Points.Add(new CubePoint { a = -a, b = 0, c = -c, e = 1 });
            Points.Add(new CubePoint { a = a, b = 0, c = c, e = 1 });
            Points.Add(new CubePoint { a = a, b = 0, c = -c, e = 1 });
            Points.Add(new CubePoint { a = 0, b = b, c = 0, e = 1 });
            Points.Add(new CubePoint { a = 0, b = -b, c = 0, e = 1 });
        }
         
        //1 => M = P(w, h).T(0,0,d).Ry(alpha)
        private void GetMatrix()
        {
            double rotationAngle = 45;
            double rotationRadians = rotationAngle * (Math.PI / 180);

            var M1 = MatrixMultiplication(FillTMatrix(0, 0, d), FillPMatrix(w, h), 4);
            var M = MatrixMultiplication(M1, FillRyMatrix(rotationRadians), 4);
        }

        private void FillVcMatrix(double[,] M)
        {
            foreach(var p in Points)
            {
                double[,] newP = { { p.a }, { p.b }, { p.c }, { p.e } };
                double[,] Vp = MatrixMultiplication(newP, M, 1);
                double[,] Vn = { { Vp[0, 0]/ Vp[0, 3] },
                    { Vp[0, 1] / Vp[0, 3] },
                    { Vp[0, 2] / Vp[0, 3] },
                    { 1.00 } };
                double[,] Vs = {{ (1 / 2) * w * (1 + Vp[0, 0])},
                    { (1 / 2) * h * (1 + Vp[0, 1])}};
            }
        }

        private double[,] FillTMatrix(double x, double y, double z)
        {
            double[,] arr = { { 1, 0, 0, x }, 
                           { 0, 1, 0, y },
                           { 0, 0, 1, z },
                           { 0, 0, 0, 1}};
            return arr;
        }

        private double[,] FillPMatrix(double w, double h)
        {
            double[,] arr = { {h/w, 0, 0, 0 },
                           { 0, 1, 0, 0 },
                           { 0, 0, 0, 1},
                           { 0, 0, -1, 0}};
            return arr;
        }

        private double[,] FillRyMatrix(double alpha)
        {
            double[,] arr = { { Math.Cos(alpha), 0, -Math.Sin(alpha), 0},
                           { 0, 1, 0, 0 },
                           { Math.Sin(alpha), 0, Math.Cos(alpha), 0 },
                           { 0, 0, 0, 1}};
            return arr;
        }

        private double[,] MatrixMultiplication(double[,] a, double[,] b, int r)
        {
            double[,] c = new double[4,4];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    c[i, j] = 0;
                    for (int k = 0; k < 4; k++)
                    {
                        c[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return c;
        }
    }
}
