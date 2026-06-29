using SkiaSharp;

namespace Ricebird.Framework.Tools
{
    public static class ImagePlaceHolder
    {
        public static byte[] ImageHolder(int width, int height, string text = "")
        {
            SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                var textPaint = new SKPaint()
                {
                    Color = new SKColor(150, 150, 150), //颜色
                    StrokeWidth = 1, //画笔宽度
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal), //字体
                    TextSize = 32,  //字体大小
                    Style = SKPaintStyle.StrokeAndFill,
                    IsAntialias = true,
                };

                var backgroundPaint = new SKPaint()
                {
                    Color = new SKColor(202, 202, 202),
                    StrokeWidth = 1,
                    Style = SKPaintStyle.StrokeAndFill,
                };

                canvas.DrawRect(0, 0, width, height, backgroundPaint);

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = $"{width} x {height}";
                }

                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);
                float xText = width / 2 - textBounds.MidX;
                float yText = height / 2 - textBounds.MidY;
                canvas.DrawText(text, xText, yText, textPaint);
            }

            MemoryStream ms = new MemoryStream();
            bitmap.Encode(ms, SKEncodedImageFormat.Jpeg, 100);

            return ms.ToArray();
        }
    }
}
