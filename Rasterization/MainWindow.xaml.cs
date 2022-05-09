using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Drawing.Imaging;
//using System.Drawing;
//using System.Drawing;

namespace Rasterization
{
    public partial class MainWindow : Window
    {
        private Point StartPoint;
        private Point EndPoint;
        int DrawingMode = 6; // 1-Line, 2-Circle, 3- Polygon, 4-Move, 5-Edit, 6-Disabled
        private List<UIElement> DrawnShapes = new List<UIElement>();
        private List<Color> ColorInfo = new List<Color>();
        private WriteableBitmap writeableBitmap;
        private List<Point> polygonPoints = new List<Point>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int rawStride = (1000 * PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * 550];
            BitmapSource bitmap = BitmapSource.Create(1000, 550,
                96, 96, PixelFormats.Bgr32, null,
                rawImage, rawStride);
            writeableBitmap = new WriteableBitmap(bitmap);

            MyCanvas.Source = writeableBitmap;
            MyCanvas.Stretch = Stretch.None;
            MyCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            MyCanvas.VerticalAlignment = VerticalAlignment.Top;

            var properties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            ColorInfo = properties.Select(prop =>
            {
                return (Color)prop.GetValue(null, null); ;
            }).ToList();

            FillColors();
        }

        private void FillColors()
        {
            for (int i = 0; i < ColorInfo.Count; i++)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Background = new SolidColorBrush(ColorInfo[i]);
                comboBoxItem.HorizontalAlignment = HorizontalAlignment.Stretch;
                comboBoxItem.Height = 20;
                comboBoxItem.HorizontalContentAlignment = HorizontalAlignment.Center;

                MyColorComboBox.Items.Add(comboBoxItem);
            }
        }

        //https://stackoverflow.com/questions/4662193/how-to-convert-from-system-drawing-color-to-system-windows-media-color
        private System.Drawing.Color ConvertColor(System.Windows.Media.Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        void SetPixel(int x, int y, Color color)
        {
            if (y > writeableBitmap.PixelHeight - 1 || x > writeableBitmap.PixelWidth - 1)
                return;
            if (y < 0 || x < 0)
                return;

            IntPtr pBackBuffer = writeableBitmap.BackBuffer;
            int stride = writeableBitmap.BackBufferStride;

            unsafe
            {
                byte* pBuffer = (byte*)pBackBuffer.ToPointer();
                int location = y * stride + x * 4;

                pBuffer[location] = color.B;
                pBuffer[location + 1] = color.G;
                pBuffer[location + 2] = color.R;
                pBuffer[location + 3] = color.A;

            }
            writeableBitmap.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        }

        private void ApplyDDA(Point startPoint, Point endPoint)
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
                SetPixel((int)startPoint.X, (int)startPoint.Y, Colors.Red);
                SetPixel((int)endPoint.X, (int)endPoint.Y, Colors.Blue);
                startPoint.X += distanceX;
                startPoint.Y += distanceY;
            }
        }

        void ApplyMidpointCircle(int R)
        {
            //int dE = 3;
            //int dSE = 5 ‐ 2 * R;
            //int d = 1‐R;
            //int x = 0;
            //int y = R;
            //putPixel(x, y);
            //while (y > x)
            //{
            //    if (d < 0) //move to E
            //    {
            //        d += dE;
            //        dE += 2;
            //        dSE += 2;
            //    }
            //    else //move to SE
            //    {
            //        d += dSE;
            //        dE += 2;
            //        dSE += 4;
            //        ‐‐y;
            //    }
            //    ++x;
            //    putPixel(x, y);
            //}
        }

        private double DistanceOfTwoPoints(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint = e.GetPosition(this);
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && DrawingMode == 1)
            {
                EndPoint = e.GetPosition(this);
            }
            if (e.LeftButton == MouseButtonState.Pressed && DrawingMode == 2)
            {
                Point center = StartPoint;
                EndPoint = e.GetPosition(this);
                double radius = DistanceOfTwoPoints(center, EndPoint);
                MidpointCircle((int)radius);
            }
        }

        private void CanvasLeftUp(object sender, MouseButtonEventArgs e)
        {
            if(DrawingMode == 1) //line
            {
                writeableBitmap.Lock();
                try
                {
                    ApplyDDA(StartPoint, EndPoint);

                }
                finally
                {
                    writeableBitmap.Unlock();
                }

                //Canvas.Children.Add(new Line(StartPoint, EndPoint));
            }
            if (DrawingMode == 2) //circle
            {

            }
            if (DrawingMode == 3) //polygon
            {

            }
            if (DrawingMode == 4) //move
            {

            }
            if (DrawingMode == 5) //edit
            {

            }
            return;
        }

        private void LineButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 1;
            
        }

        private void CircleButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 2;
        }

        private void PolygonButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 3;
        }

        private void MoveButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 4;
        }

        private void EditButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 5;
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
