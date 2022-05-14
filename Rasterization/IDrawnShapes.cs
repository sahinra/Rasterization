using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rasterization
{
    interface IDrawnShapes
    {
        string Name { get; set; }
        Color Color { get; set; }
        WriteableBitmap WriteableBitmap { get; set; }
        List<Point> GetPoints();
        void DeleteShape();
        void Redraw(int x, int y);
        void MoveShape();
        void EditShape(int x, int y, int index);
    }
}
