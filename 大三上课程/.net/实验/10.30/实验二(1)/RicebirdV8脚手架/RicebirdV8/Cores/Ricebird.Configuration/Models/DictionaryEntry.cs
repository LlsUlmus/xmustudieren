using System.ComponentModel.DataAnnotations;

namespace Ricebird.Configuration.Models
{
    public class DictionaryEntry : AscendingEntityBase, IValidatable
    {
        #region 数据库字段
        [Required, MaxLength(20)]
        public string DataKey
        {
            get; set;
        } = string.Empty;

        [Required, MaxLength(30)]
        public string DataValue
        {
            get; set;
        } = string.Empty;

        public Guid DataDictionaryId
        {
            get; set;
        } = Guid.Empty;

        public bool CanEdit
        {
            get; set;
        } = false;

        public bool CanDelete
        {
            get; set;
        } = false;

        public bool Visible
        {
            get; set;
        } = true;

        public bool Enable
        {
            get; set;
        } = true;
        #endregion

        public void SetPermission(DictionaryFrom from)
        {
            (CanEdit, CanDelete) = from switch
            {
                DictionaryFrom.Enum => (false, false),
                DictionaryFrom.EnsureFunction => (true, true),
                _ => (true, true),
            };
        }

        public FluentValidator BuildValidator()
        {
            FluentValidator<DictionaryEntry> fluent = new FluentValidator<DictionaryEntry>();
            fluent.AutoRulesByAttributes();
            fluent.RuleFor(e => e.DataKey).Should((c, r, o) =>
            {
                DataDictionaryService dictSerivce = (c.Resolve<IDataDictionaryService>() as DataDictionaryService)!;

                var dict = dictSerivce[o.DataDictionaryId];
                if (dict is null)
                {
                    r.SetFailure(nameof(DataDictionaryId), $"不存在一个ID为{o.DataDictionaryId}的字典");
                    return;
                }

                if (!dict.CanEdit)
                {
                    r.SetFailure(nameof(DataKey), $"该项所在的字典不允许修改。");
                }

                SetPermission(dict.From);

                var entry = dict[o.DataKey];

                if (entry is not null && entry.ID != o.ID)
                {
                    r.SetFailure(nameof(DataKey), $"当前已经存在另一个键为{o.DataKey}的项，无法继续使用此键名。");
                }

                if (entry is not null && !entry.CanEdit)
                {
                    r.SetFailure(nameof(DataKey), $"不允许修改此项的键。");
                }
            });
            return fluent;
        }

        public override string ToString() => $"({DataKey}/{DataValue})";
    }
}
