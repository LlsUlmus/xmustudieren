using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Ricebird.Framework.Controllers.RicebirdResults
{
    public class RicebirdBitmapResult : ActionResult
    {
        public RicebirdBitmapResult(SKBitmap bitmap, SKEncodedImageFormat format, int quality) => (Bitmap, Format, Quality) = (bitmap, format, quality);

        public SKBitmap Bitmap { get; set; }
        SKEncodedImageFormat Format { get; set; }
        public int Quality { get; set; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            MemoryStream ms = new MemoryStream();
            Bitmap.Encode(ms, Format, Quality);

            var response = context.HttpContext.Response;
            response.ContentType = Format switch
            {
                SKEncodedImageFormat.Jpeg => "image/jpeg",
                SKEncodedImageFormat.Gif => "image/gif",
                SKEncodedImageFormat.Png => "image/png",
                SKEncodedImageFormat.Ico => "image/x-icon",
                _ => ""
            };
            await response.Body.WriteAsync(ms.ToArray());
        }
    }
}
