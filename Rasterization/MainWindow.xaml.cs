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

namespace Rasterization
{
    public partial class MainWindow : Window
    {
        private Point StartPoint = new Point();
        private Point EndPoint = new Point();
        private List<IShape> SelectedShape = new List<IShape>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void CanvasLeftUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LineButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void CircleButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void PolygonButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void MoveButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void EditButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {

        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
