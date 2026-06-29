using Ricebird.Framework.Clients;

namespace Ricebird.Framework.DataValidator.Rules
{
    public class DictRule<T> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;

        public override string RuleName => "数据字典枚举验证";
        public string[] AllowValue { get; set; } = [];
        public new string Message => $"该字段的值必须是{AllowValue.JoinAsString("，")}其中之一";
        public DictRule(string dictName)
        {
            var dictService = HostEnv.ServiceProvider.Resolve<IDataDictionaryService>();
            var dict = dictService.GetDictionary(dictName);
            if (dict == null)
            {
                return;
            }

            AllowValue = dict.Select(e => e.Key).ToArray();
        }
        public override object ToJsonObject(string fieldName) => new
        {
            type = "enum",
            @enum = $"[{AllowValue.Select(e => $"'{e.Replace("'", @"\'")}'").JoinAsString(",")}]",
            message = Message
        };

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            if (value == null)
            {
                result.SetFailure(propertyName, $"字段{propertyName}不可为null");
                return;
            }

            if (!AllowValue.Contains(value))
            {
                result.SetFailure(propertyName, Message);
            }
        }
    }

    public class EnumRule<T, TEnumType> : AbstactValidateRule<T>
    {
        public override bool Multiple => false;

        public override string RuleName => "字符串枚举验证";

        public TEnumType[] AllowValue { get; set; }

        public new string Message
        {
            get; set;
        }

        public EnumRule(TEnumType[] allows)
            : this(allows, "")
        {

        }

        public EnumRule(TEnumType[] allows, string message)
        {
            AllowValue = allows;
            Message = message.IsNullOrWhiteSpace() ? $"该字段的值必须是{AllowValue.JoinAsString("，")}其中之一" : message;
        }

        public override object ToJsonObject(string fieldName)
        {
            return new
            {
                type = "enum",
                @enum = $"[{AllowValue.Select(e => $"'{(e?.ToString() ?? "").Replace("'", @"\'")}'").JoinAsString(",")}]",
                message = Message
            };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            if (value == null)
            {
                result.SetFailure(propertyName, $"字段{propertyName}不可为null");
                return;
            }

            if (value is not TEnumType finalValue)
            {
                result.SetFailure(propertyName, $"字段{propertyName}无法转换为目标类型");
                return;
            }

            if (!AllowValue.Contains(finalValue))
            {
                result.SetFailure(propertyName, Message);
            }
        }
    }
}
