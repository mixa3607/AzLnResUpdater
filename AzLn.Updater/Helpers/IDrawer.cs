using System.Drawing;

namespace AzLn.Updater
{
    public interface IDrawer
    {
        Bitmap DrawText(params string[] lines);
    }
}