using System.Drawing;
using System.Drawing.Text;
using AzLn.Updater.Options;

namespace AzLn.Updater
{
    public class Drawer : IDrawer
    {
        private readonly IDrawerOptions _options;

        public Drawer(IDrawerOptions options)
        {
            _options = options;
        }

        public Bitmap DrawText(params string[] lines)
        {
            var fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile(_options.FontPath);
            var font = new Font(fontCollection.Families[0], _options.FontSize);

            var bitmap = new Bitmap(_options.Width, _options.Height);
            using var drawing = Graphics.FromImage(bitmap);
            drawing.FillRectangle(new SolidBrush(Color.Black), 0, 0, bitmap.Width, bitmap.Height);

            var pxPerLine = bitmap.Height / lines.Length;
            for (int i = 0; i < lines.Length; i++)
            {
                var xCenter = bitmap.Width / 2;
                var yCenter = pxPerLine / 2 + pxPerLine * i;
                var sz = drawing.MeasureString(lines[i], font);
                var startDrawPoint = new PointF(xCenter - sz.Width / 2, yCenter - sz.Height / 2);
                drawing.DrawString(lines[i], font, new SolidBrush(Color.White), startDrawPoint);
            }

            return bitmap;
        }
    }
}