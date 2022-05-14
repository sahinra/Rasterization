using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace Rasterization
{
    public partial class MainWindow : Window
    {
        private Point StartPoint;
        private Point EndPoint;
        int DrawingMode = 8; // 1-Line, 2-Circle, 3- Polygon, 4-Move, 5-Edit, 6-Delete, 7-Clear All, 8-Disabled
        private List<IDrawnShapes> DrawnShapes = new List<IDrawnShapes>();
        private List<Color> ColorInfo = new List<Color>();
        private WriteableBitmap writeableBitmap;
        private List<Point> polygonPoints = new List<Point>();
        private Color SelectedColor = Colors.Red;
        private int PolygonPointNum = 5;
        private int selectedIndex = 0;
        private Point selectedPoint = new Point(); //point to edit

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

        private int MeasureDistance(Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private Point FindNearestPoint(Point point, List<Point> points)
        {
            int min = MeasureDistance(point, points[0]);
            Point nearestPoint = points[0];
            foreach (var p in points)
            {
                if(min > MeasureDistance(point, p))
                {
                    min = MeasureDistance(point, p);
                    nearestPoint = p;
                }
            }
            return nearestPoint;
        }

        private int FindMinDistance(Point point, List<Point> points)
        {
            int min = MeasureDistance(point, points[0]);

            foreach (var p in points)
            {
                if (min > MeasureDistance(point, p))
                {
                    min = MeasureDistance(point, p);
                }
            }
            return min;
        }

        private int FindNearestShape()
        {
            int indexShape = 0;
            int currDist;

            if (DrawnShapes.Count == 0)
            {
                return -1;
            }

            int minDist = FindMinDistance(StartPoint, DrawnShapes[0].GetPoints());

            foreach (IDrawnShapes o in DrawnShapes)
            {
                currDist = FindMinDistance(StartPoint, o.GetPoints());
                if (currDist < minDist)
                {
                    minDist = currDist;
                    indexShape = DrawnShapes.IndexOf(o);
                }
            }
            //MessageBox.Show(indexShape.ToString());
            //MessageBox.Show(DrawnShapes.Count.ToString());
            return indexShape;
        }

        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint = e.GetPosition(this);
            if(DrawingMode == 3) //polygon
            {
                if (polygonPoints.Count < PolygonPointNum)
                {
                    Point p = e.GetPosition(this);
                    polygonPoints.Add(p);
                }
                else return;
            }
            if (DrawingMode == 4) //move
            {
                selectedIndex = FindNearestShape();
            }
            if (DrawingMode == 5) //edit
            {
                selectedIndex = FindNearestShape();
                selectedPoint = FindNearestPoint(StartPoint, DrawnShapes[selectedIndex].GetPoints());
            }
            if (DrawingMode == 6) //delete
            {

            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                EndPoint = e.GetPosition(this);
            }
        }

        private void CanvasLeftUp(object sender, MouseButtonEventArgs e)
        {
            if (DrawingMode == 1) //line
            {
                writeableBitmap.Lock();
                try
                {
                    DrawLine line = new DrawLine(StartPoint, EndPoint);
                    line.WriteableBitmap = writeableBitmap;
                    line.ApplyDDA(SelectedColor);
                    DrawnShapes.Add(line);
                }
                finally
                {
                    writeableBitmap.Unlock();
                }
            }
            if (DrawingMode == 2) //circle
            {
                writeableBitmap.Lock();
                try
                {
                    DrawCircle circle = new DrawCircle(StartPoint, EndPoint);
                    circle.WriteableBitmap = writeableBitmap;
                    circle.ApplyMidpointCircle(SelectedColor);
                    DrawnShapes.Add(circle);
                }
                finally
                {
                    writeableBitmap.Unlock();
                }
            }
            if (DrawingMode == 3) //polygon
            {
                if (polygonPoints.Count == PolygonPointNum)
                {
                    writeableBitmap.Lock();
                    try
                    {
                        DrawPolygon polygon = new DrawPolygon(polygonPoints);
                        polygon.WriteableBitmap = writeableBitmap;
                        polygon.ApplyModifiedDDA(SelectedColor);
                        DrawnShapes.Add(polygon);
                        polygonPoints.Clear();
                    }
                    finally
                    {
                        writeableBitmap.Unlock();
                    }
                }
            }
            if (DrawingMode == 4) //move
            {
                DrawnShapes[selectedIndex].MoveShape((int)EndPoint.X, (int)EndPoint.Y);
            }
            if (DrawingMode == 5) //edit
            {
                DrawnShapes[selectedIndex].EditShape((int)EndPoint.X, (int)EndPoint.Y, DrawnShapes[selectedIndex].GetPoints().IndexOf(selectedPoint));
            }
            if (DrawingMode == 6) //delete
            {
                int index = FindNearestShape();
                DrawnShapes[index].DeleteShape();
                DrawnShapes.RemoveAt(index);
            }
            if (DrawingMode == 7) //clear all
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
            DrawingMode = 6;
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            foreach(var o in DrawnShapes.ToList())
            {
                o.DeleteShape();
                DrawnShapes.Remove(o);
            }
        }
    }
}
