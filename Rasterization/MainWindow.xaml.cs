using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Rasterization
{
    public partial class MainWindow : Window
    {
        private Point StartPoint;
        private Point EndPoint;
        int DrawingMode = 11; // 1-Line, 2-Circle, 3- Polygon, 4-Move, 5-Edit, 6-Delete, 7-Clear All, 8-Rectangle,
                             // 9-Select Rect, 10-Select Poly, 11-Disabled, 
        private int selectedIndex = 0;

        private List<IDrawnShapes> DrawnShapes = new List<IDrawnShapes>();
        private List<Color> ColorInfo = new List<Color>();
        private Color SelectedColor = Colors.Red;
        private Color SelectedBackColor = Colors.HotPink;
        private DrawRectangle SelectedRect;
        private DrawPolygon SelectedPoly;

        private WriteableBitmap writeableBitmap;
        private List<Point> polygonPoints = new List<Point>();     
        private Point selectedPoint = new Point(); //point to edit
        private bool polygonPointsDone = false;

        public MainWindow()
        {
            InitializeComponent();
            MyColorComboBox.SelectionChanged += new SelectionChangedEventHandler(ColorComboBoxSelectionChanged);
            MyColorComboBox2.SelectionChanged += new SelectionChangedEventHandler(ColorComboBox2SelectionChanged);
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

                ComboBoxItem comboBoxItem2 = new ComboBoxItem();
                comboBoxItem2.Background = new SolidColorBrush(ColorInfo[i]);
                comboBoxItem2.HorizontalAlignment = HorizontalAlignment.Stretch;
                comboBoxItem2.Height = 20;
                comboBoxItem2.HorizontalContentAlignment = HorizontalAlignment.Center;

                MyColorComboBox.Items.Add(comboBoxItem);
                MyColorComboBox2.Items.Add(comboBoxItem2);
            }
        }

        void ColorComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelectedIndex = MyColorComboBox.SelectedIndex;
            SelectedColor = ColorInfo[currentSelectedIndex];
            DrawnShapes[selectedIndex].Color = SelectedColor;
            DrawnShapes[selectedIndex].DeleteShape();
            DrawnShapes[selectedIndex].Draw(SelectedColor);
        }

        void ColorComboBox2SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelectedIndex = MyColorComboBox2.SelectedIndex;
            SelectedBackColor = ColorInfo[currentSelectedIndex];
            SelectedPoly.FilledColor = SelectedBackColor;
            SelectedPoly.DeleteShape();
            SelectedPoly.IsFilledColor = true;
            SelectedPoly.Draw(SelectedPoly.Color);
            if (SelectedPoly.IsFilledColor)
            {
                SelectedPoly.FillPolygon(SelectedPoly.FilledColor);
            }
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
            return indexShape;
        }

        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            StartPoint = e.GetPosition(MyCanvas);
            if (DrawingMode == 3) //polygon
            {
                if (polygonPoints.Count == 0 && polygonPointsDone == false)
                {
                    Point p = e.GetPosition(MyCanvas);
                    polygonPoints.Add(p);
                    return;
                }

                if (MeasureDistance(StartPoint, polygonPoints[0]) > 100 && polygonPointsDone == false)
                {
                    Point p1 = e.GetPosition(MyCanvas);
                    polygonPoints.Add(p1);
                    return;
                }
                else
                {
                    polygonPointsDone = true;
                    return;
                }
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
            if (DrawingMode == 9)
            {
                selectedIndex = FindNearestShape();
                if (DrawnShapes[selectedIndex].Name == "Rectangle")
                {
                    SelectedRect = (DrawRectangle)DrawnShapes[selectedIndex];
                    Debug.WriteLine("Rectangle selected!");
                }
            }
            if (DrawingMode == 10)
            {
                selectedIndex = FindNearestShape();
                if (DrawnShapes[selectedIndex].Name == "Polygon")
                {
                    SelectedPoly = (DrawPolygon)DrawnShapes[selectedIndex];
                    Debug.WriteLine("Polygon selected!");
                }
            }
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || DrawingMode != 3)
            {
                EndPoint = e.GetPosition(MyCanvas);
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
                    line.Color = SelectedColor;
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
                    circle.Color = SelectedColor;
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
                Debug.WriteLine(polygonPointsDone);
                if (polygonPointsDone == true)
                {
                    writeableBitmap.Lock();
                    try
                    {
                        DrawPolygon polygon = new DrawPolygon(polygonPoints);
                        polygon.WriteableBitmap = writeableBitmap;
                        polygon.Color = SelectedColor;
                        polygon.ApplyModifiedDDA(SelectedColor);
                        DrawnShapes.Add(polygon);
                        polygonPoints.Clear();
                        polygonPointsDone = false;
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
            if (DrawingMode == 8) //rect
            {
                writeableBitmap.Lock();
                try
                {
                    DrawRectangle rect = new DrawRectangle(StartPoint, EndPoint);
                    rect.WriteableBitmap = writeableBitmap;
                    rect.Color = SelectedColor;
                    rect.RectangleDrawing(SelectedColor);
                    DrawnShapes.Add(rect);
                }
                finally
                {
                    writeableBitmap.Unlock();
                }
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

        private void RectButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 8;
        }

        private void SelectRectButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 9;
        }

        private void SelectPolyButtonClick(object sender, RoutedEventArgs e)
        {
            DrawingMode = 10;
        }

        private void ClipButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedPoly.LiangBarsky(SelectedRect.LeftTop, SelectedRect.RightBottom, SelectedRect);
        }

        private void FillButtonClick(object sender, RoutedEventArgs e)
        {
            if(SelectedPoly != null)
            {
                if(SelectedBackColor != null)
                {
                    SelectedPoly.FillPolygon(Colors.Blue);
                    Debug.WriteLine("inside");
                }
                Debug.WriteLine("helloo");
            }
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
            MySlider.Value = 0;
        }

        public struct SeralizedShapes {
            public string Name;
            public List<Point> Points;
            public Color Color;
            public Color BackColor;
            public int Thickness;
            public bool FilledColor;
            public bool FilledImage;
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Document";
            saveFileDialog.Filter = "XML files(.xml)|*.xml|all Files(*.*)|*.*";

            List<SeralizedShapes> seralizedShapes = new List<SeralizedShapes>();
            foreach(var item in DrawnShapes)
            {
                SeralizedShapes newItem = new SeralizedShapes
                {
                    Name = item.Name,
                    Points = item.GetPoints(),
                    Color = item.Color,
                    BackColor = item.FilledColor,
                    Thickness = item.Thickness,
                    FilledColor = item.IsFilledColor,
                    FilledImage = item.IsFilledImage
                };

                seralizedShapes.Add(newItem);
            }

            if(saveFileDialog.ShowDialog() == true)
            {
                FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);

                XmlSerializer xmlSerializer = new XmlSerializer(seralizedShapes.GetType());
                xmlSerializer.Serialize(fileStream, seralizedShapes);
                fileStream.Close();
            }
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "Document";
            dialog.Filter = "XML files(.xml)|*.xml|all Files(*.*)|*.*";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read);
                var xmlSerializer = new XmlSerializer(new List<SeralizedShapes>().GetType());

                var items = (List<SeralizedShapes>)xmlSerializer.Deserialize(stream);
                stream.Close();

                foreach (var o in DrawnShapes.ToList())
                {
                    o.DeleteShape();
                    DrawnShapes.Remove(o);
                }

                foreach (var item in items)
                {
                    if(item.Name == "Line")
                    {
                        writeableBitmap.Lock();
                        try
                        {
                            DrawLine line = new DrawLine(item.Points[0], item.Points[1]);
                            line.WriteableBitmap = writeableBitmap;
                            line.Thickness = item.Thickness;
                            line.ApplyDDA(item.Color);                          
                            DrawnShapes.Add(line);
                        }
                        finally
                        {
                            writeableBitmap.Unlock();
                        }
                    }
                    if(item.Name == "Circle")
                    {
                        writeableBitmap.Lock();
                        try
                        {
                            DrawCircle circle = new DrawCircle(item.Points[0], item.Points[1]);
                            circle.WriteableBitmap = writeableBitmap;
                            circle.Thickness = item.Thickness;
                            circle.ApplyMidpointCircle(item.Color);                          
                            DrawnShapes.Add(circle);
                        }
                        finally
                        {
                            writeableBitmap.Unlock();
                        }
                    }
                    if(item.Name == "Polygon")
                    {
                        writeableBitmap.Lock();
                        try
                        {
                            DrawPolygon polygon = new DrawPolygon(item.Points);
                            polygon.WriteableBitmap = writeableBitmap;
                            polygon.Thickness = item.Thickness;
                            polygon.FilledColor = item.BackColor;
                            polygon.ApplyModifiedDDA(item.Color);
                            if (polygon.FilledColor != null)
                                polygon.FillPolygon(item.BackColor);
                            if (polygon.FilledImage != null)
                                polygon.FillWithImage();
                            DrawnShapes.Add(polygon);                           
                        }
                        finally
                        {
                            writeableBitmap.Unlock();
                        }
                    }
                    if (item.Name == "Rectangle")
                    {
                        writeableBitmap.Lock();
                        try
                        {
                            DrawRectangle rect = new DrawRectangle(item.Points[0], item.Points[2]);
                            rect.WriteableBitmap = writeableBitmap;
                            rect.Thickness = item.Thickness;
                            rect.RectangleDrawing(item.Color);
                            DrawnShapes.Add(rect);
                        }
                        finally
                        {
                            writeableBitmap.Unlock();
                        }
                    }
                }
            }
        }

        private void MySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(DrawnShapes.Count > 0)
            {
                DrawnShapes[selectedIndex].DeleteShape();
                DrawnShapes[selectedIndex].Thickness = (int)MySlider.Value;
                DrawnShapes[selectedIndex].Draw(SelectedColor);
            }
        }

        private System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }

        private void FillImageButtonClick(object sender, RoutedEventArgs e)
        {

            var dialog = new OpenFileDialog();
            dialog.FileName = "Document";
            dialog.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp;*.tiff|" +
            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            "Portable Network Graphic (*.png)|*.png";

            bool? result = dialog.ShowDialog();

            if (SelectedPoly == null)
            {
                return;
            }

            if (result == true)
            {              
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(dialog.FileName);
                bitmap.EndInit();
                SelectedPoly.FilledImage = BitmapImage2Bitmap(bitmap);
            }

            SelectedPoly.FillWithImage();
        }
    }
}
