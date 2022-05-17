using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Xml;
using Microsoft.Win32;

namespace Rasterization
{
    public partial class MainWindow : Window
    {
        private Point StartPoint;
        private Point EndPoint;
        int DrawingMode = 8; // 1-Line, 2-Circle, 3- Polygon, 4-Move, 5-Edit, 6-Delete, 7-Clear All, 8-Disabled
        private int PolygonPointNum = 5;
        private int selectedIndex = 0;

        private List<IDrawnShapes> DrawnShapes = new List<IDrawnShapes>();
        private List<Color> ColorInfo = new List<Color>();
        private Color SelectedColor = Colors.Red;

        private WriteableBitmap writeableBitmap;
        private List<Point> polygonPoints = new List<Point>();     
        private Point selectedPoint = new Point(); //point to edit
        private int AntialiasOnOff = 0; //off

        public MainWindow()
        {
            InitializeComponent();
            MyColorComboBox.SelectionChanged += new SelectionChangedEventHandler(ColorComboBoxSelectionChanged);
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

        void ColorComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelectedIndex = MyColorComboBox.SelectedIndex;
            SelectedColor = ColorInfo[currentSelectedIndex];
            DrawnShapes[selectedIndex].DeleteShape();
            DrawnShapes[selectedIndex].Draw(SelectedColor);
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
                    circle.midpoint(SelectedColor);
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

        private void AntialiasButtonClick(object sender, RoutedEventArgs e)
        {
            if(AntialiasOnOff == 0)
            {
                AntialiasOnOff = 1;
                aliasButtonText.Text = "Antialias On";
            }
            else
            {
                AntialiasOnOff = 0;
                aliasButtonText.Text = "Antialias Off";
            }


        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            XmlTextWriter writer = new XmlTextWriter("D:/Computer Sciences/Semester 6/Computer Graphics/shapes.xml", System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;

            writer.WriteStartElement("Shapes");

            foreach(IDrawnShapes shape in DrawnShapes)
            {
                if ( shape.Name == "Line")
                {
                    CreateNode("Line", shape.GetPoints(), shape.Thickness, shape.Color, writer);
                }
                if (shape.Name == "Circle")
                {
                    CreateNode("Circle", shape.GetPoints(), shape.Thickness, shape.Color, writer);
                }
                if (shape.Name == "Polygon")
                {
                    CreateNode("Polygon", shape.GetPoints(), shape.Thickness, shape.Color, writer);
                }
            }

            writer.WriteEndElement(); //shapes
            writer.WriteEndDocument();
            writer.Close();
            _ = MessageBox.Show("XML File created ! ");
        }

        private void CreateNode(string name, List<Point> points, int thickness, Color color, XmlTextWriter writer)
        {
            writer.WriteStartElement(name);
            writer.WriteStartElement("Points");
            foreach(Point point in points)
            {
                writer.WriteStartElement("Point");
                writer.WriteStartElement("Point_X");
                writer.WriteString(point.X.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("Point_Y");
                writer.WriteString(point.Y.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement(); //point
            }
            writer.WriteStartElement("Color");
            writer.WriteStartElement("R");
            writer.WriteString(color.R.ToString());
            writer.WriteEndElement();
            writer.WriteStartElement("G");
            writer.WriteString(color.G.ToString());
            writer.WriteEndElement();
            writer.WriteStartElement("B");
            writer.WriteString(color.B.ToString());
            writer.WriteEndElement();
            writer.WriteEndElement(); //color
            writer.WriteStartElement("Thickness");
            writer.WriteString(thickness.ToString());
            writer.WriteEndElement();

            writer.WriteEndElement(); //points
            writer.WriteEndElement(); //name
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.FileName = "Document";
            dialog.Filter = "XML files(.xml)|*.xml|all Files(*.*)|*.*";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                foreach (var o in DrawnShapes.ToList())
                {
                    //o.DeleteShape();
                    //DrawnShapes.Remove(o);
                    //ReadXmlValues(dialog.FileName);
                }
            }
        }

        private void ReadXmlValues(string filename)
        {
            using (XmlReader reader = XmlReader.Create(filename))
            {
                reader.Read();
                reader.ReadStartElement("Shapes");

                if (reader.ReadString() == "Line")
                {
                    reader.ReadStartElement("Line");

                    reader.ReadEndElement();
                }
                reader.ReadStartElement("Line");
                Console.Write("The content of the title element:  ");
                Console.WriteLine(reader.ReadString());
                reader.ReadEndElement();
                reader.ReadStartElement("price");
                Console.Write("The content of the price element:  ");
                Console.WriteLine(reader.ReadString());
                reader.ReadEndElement();
                reader.ReadEndElement(); //shapes
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
    }
}
