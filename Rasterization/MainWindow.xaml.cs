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

namespace Rasterization
{
    public partial class MainWindow : Window
    {
        private Point StartPoint;
        private Point EndPoint;
        private Point CurrentPoint;
        private Line newLine;
        int DrawingMode = 6; // 1-Line, 2-Circle, 3- Polygon, 4-Move, 5-Edit, 6-Disabled
        private List<UIElement> DrawnShapes = new List<UIElement>();
        private List<Color> ColorInfo = new List<Color>();
        private WriteableBitmap bmp = new WriteableBitmap(
                1000,
                550,
                96,
                96,
                PixelFormats.Bgr32,
                null);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //bmp = new System.Drawing.Bitmap((int)Canvas.Width, (int)Canvas.Height);
            MyCanvas.Source = bmp;

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

        //private static System.Drawing.Bitmap GetArgbCopy(System.Drawing.Image sourceImage)
        //{
        //    System.Drawing.Bitmap bitmapSource = GetArgbCopy(MyCanvas);

        //    using (Graphics graphics = Graphics.FromImage(bmpNew))
        //    {
        //        graphics.DrawImage(sourceImage, new System.Drawing.Rectangle(0, 0, bmpNew.Width, bmpNew.Height),
        //            new System.Drawing.Rectangle(0, 0, bmpNew.Width, bmpNew.Height), GraphicsUnit.Pixel);
        //        graphics.Flush();
        //    }
        //    return bmpNew;
        //}

        private void ApplyDDA(Point startPoint, Point endPoint)
        {
            double distanceX = endPoint.X - startPoint.X;
            double distanceY = endPoint.Y - startPoint.Y;
            double step;
            double slope = (endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X);
            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(1000, 550);

            BitmapImage bmp = new BitmapImage();


            //System.Drawing.Color newColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

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
                //bmp.SetPixel((int)startPoint.X, (int)startPoint.Y, ConvertColor(Colors.Black));
                startPoint.X += distanceX;
                startPoint.Y += distanceY;
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

        void MidpointCircle(int R)
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
                //Line line = new Line();

                //line.Stroke = SystemColors.WindowFrameBrush;
                //line.X1 = CurrentPoint.X;
                //line.Y1 = CurrentPoint.Y;
                //line.X2 = e.GetPosition(this).X;
                //line.Y2 = e.GetPosition(this).Y;

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
                //newLine = new Line();
                //newLine.Stroke = SystemColors.WindowFrameBrush;
                //newLine.X1 = StartPoint.X;
                //newLine.Y1 = StartPoint.Y;
                //newLine.X2 = EndPoint.X;
                //newLine.Y2 = EndPoint.Y;


                bmp.Lock();
                try
                {
                    ApplyDDA(StartPoint, EndPoint);
                    //foreach (var point in line.Points)
                    //{
                    //    SetPixel(point.X, point.Y, line.Color);
                    //    line.Brush.Center = point;
                    //    line.Brush.CalculatePoints();
                    //    Draw(line.Brush);
                        

                    //}
                }
                finally
                {
                    bmp.Unlock();
                }

                //Canvas.Children.Add(new Line(StartPoint, EndPoint));
            }
            if (DrawingMode == 2) //circle
            {

            }
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
            //foreach (var o in MyCanvas.Children)
            //{
            //    MyCanvas.Children.Remove((UIElement)o);
            //}
        }
    }
}
