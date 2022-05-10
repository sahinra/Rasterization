using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rasterization
{
    interface IDrawnShapes
    {
        string Name { get; set; }
        Color Color { get; set; }
        WriteableBitmap WriteableBitmap { get; set; }
        void DeleteShape();
        void MoveShape();
        void EditShape();
    }
}
