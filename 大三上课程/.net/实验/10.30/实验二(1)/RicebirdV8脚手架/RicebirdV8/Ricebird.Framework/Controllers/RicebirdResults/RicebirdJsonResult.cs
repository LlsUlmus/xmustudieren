using Microsoft.AspNetCore.Mvc;
using Ricebird.Framework.Database;
using Ricebird.Framework.Tools.JsonConverters;

namespace Ricebird.Framework.Controllers.RicebirdResults
{
    public class RicebirdJsonResult : JsonResult
    {
        public RicebirdJsonResult(object value, bool allowGetInProduction, string formatter) : base(value)
        {
            DateTimeFormat = formatter;
            Value = value;
            AllowGetInProduction = allowGetInProduction;
            Option = RicebirdSerializerOption.Default;
            var dateTimeConverter = new RicebirdDateTimeConverter(formatter);
            Option.Converters.Add(dateTimeConverter);
        }

        public string DateTimeFormat
        {
            get; set;
        }

        public bool AllowGetInProduction
        {
            get; set;
        } = false;

        private JsonSerializerOptions Option { get; init; }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            HostEnv env = context.HttpContext.RequestServices.Resolve<HostEnv>();

            if (!AllowGetInProduction && !env.FrameworkOptions.AlwaysAllowGet && env.IsProduction() && context.HttpContext.Request.Method == "GET")
            {
                Value = new
                {
                    success = false,
                    msg = "无法通过GET访问此接口。"
                };
            }

            if (env.IsDevelopment() || env.FrameworkOptions.ShowSqlInApi)
            {
                RicebirdContext ctx = context.HttpContext.RequestServices.Resolve<RicebirdContext>();
                Dictionary<string, object> obj = (Value ?? new { }).ObjectToDictionary();
                var sql = ctx.DbDiagnostic;
                obj.MergeKey("sql", sql);
                Value = obj;
            }

            if (env.IsDevelopment())
            {
                Option.WriteIndented = true;
            }

            var response = context.HttpContext.Response;
            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

            string json = JsonSerializer.Serialize(Value, Option);
            await context.HttpContext.Response.WriteAsync(json);
        }


    }
}
