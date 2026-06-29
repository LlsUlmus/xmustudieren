using Microsoft.AspNetCore.Mvc;
using System.Resources;
using System.Web;

namespace Ricebird.Framework.Tools
{
    public static class ResultPages
    {
        public const string WARNING = "warning";
        public const string ERROR = "error";
        public const string SUCCESS = "success";
        public const string INFO = "info";

        public static ActionResult ResultPage(string type, string message)
        {
            ResourceManager rm = new ResourceManager("Ricebird.Framework.Resource", typeof(ResultPages).Assembly);
            string page = rm.GetString($"result-{type}") ?? "";
            string result = page.Replace("{{ result-title }}", message);
            return new ContentResult()
            {
                Content = result,
                ContentType = "text/html;charset=utf-8",
                StatusCode = 200,
            };
        }

        public static async Task WarningPageAsync(string message, HttpContext context)
        {
            await ToPageAsync(WARNING, message, context);
        }

        public static async Task InfoPageAsync(string message, HttpContext context)
        {
            await ToPageAsync(INFO, message, context);
        }

        public static async Task SuccessPageAsync(string message, HttpContext context)
        {
            await ToPageAsync(SUCCESS, message, context);
        }

        public static async Task ErrorPageAsync(string message, HttpContext context)
        {
            await ToPageAsync(ERROR, message, context);
        }

        public static async Task ToFileAsync(byte[] bytes, string mimeType, string fileName, HttpContext context, string displayName)
        {
            var response = context.Response;
            response.ContentType = mimeType;
            if (!response.ContentType.StartsWith("image") && mimeType != "application/pdf")
            {
                displayName = displayName.HasValue() ? displayName : fileName;
                response.Headers.ContentDisposition = $"attachment; filename={HttpUtility.UrlEncode(displayName, Encoding.UTF8)}";
                response.Headers.ContentLength = bytes.Length;
            }
            await response.BodyWriter.WriteAsync(bytes);
        }

        private static async Task ToPageAsync(string type, string message, HttpContext context)
        {
            var response = context.Response;
            ResourceManager rm = new ResourceManager("Ricebird.Framework.Resource", typeof(ResultPages).Assembly);
            string page = rm.GetString($"result-{type}") ?? "";
            string result = page.Replace("{{ result-title }}", message);
            response.ContentType = "text/html;charset=utf-8";
            await response.BodyWriter.WriteAsync(result.GetBytes());
        }
    }
}
