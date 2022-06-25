using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rasterization
{
    public interface IDrawnShapes
    {
        string Name { get; set; }
        Color Color { get; set; }
        int Thickness { get; set; }
        Color FilledColor { get; set; }
        bool IsFilledImage { get; set; }
        bool IsFilledColor { get; set; }
        WriteableBitmap WriteableBitmap { get; set; }
        void Draw(Color color);
        List<Point> GetPoints();
        void DeleteShape();
        void MoveShape(int x, int y);
        void EditShape(int x, int y, int index);
        void FillPolygon(Color color);
    }
}
