using Ricebird.Framework.Clients;
using Ricebird.Framework.DataValidator;
using Ricebird.Framework.DataValidator.Rules;

namespace Ricebird.Framework.FileStorage.DataValidatorExtensions
{
    public class AttachmentMustExistsRule<T>(string usage, Predicate<IClient> condition) : AbstactValidateRule<T>
    {
        public override bool Multiple => true;

        public override string RuleName => "附件必须存在";

        public override object ToJsonObject(string fieldName)
        {
            return new { };
        }

        public override void Validate(IClient client, ValidateResult result, T validateObj, string propertyName, string fieldName, object? value)
        {
            IFileStorageService fileStorage = client.Resolve<IFileStorageService>();
            List<string> attachList = client.GetList<string>(usage, "|");
            List<PermanentFile> finalFile = [];
            List<string> invalidIds = [];
            foreach (string strId in attachList)
            {
                if (!strId.TryParseToGuid(out Guid id))
                {
                    invalidIds.Add(strId);
                }
                else
                {
                    var file = fileStorage.GetFile(id);
                    if (file is PermanentFile f)
                    {
                        finalFile.Add(f);
                    }
                    else
                    {
                        invalidIds.Add(strId);
                    }
                }
            }

            if (condition(client) && finalFile.Count == 0)
            {
                string notExists = $"，找不到ID为{invalidIds.JoinAsString("，")}的文件";
                result.SetFailure(propertyName, $"必须上传至少一个{usage}{(invalidIds.Any() ? notExists : "")}。");
            }
        }
    }
}
