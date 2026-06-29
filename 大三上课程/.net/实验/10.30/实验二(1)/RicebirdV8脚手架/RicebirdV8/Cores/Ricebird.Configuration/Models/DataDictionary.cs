using System.ComponentModel.DataAnnotations;

namespace Ricebird.Configuration.Models
{
    public class DataDictionary : OrderedEntityBase, IValidatable, IComparable<DataDictionary>
    {
        public DataDictionary() { }
        public DataDictionary(string name, DictionaryFrom from, int order)
        {
            Name = name;
            From = from;
            DisplayOrder = order;
            (CanEdit, CanDelete) = From switch
            {
                DictionaryFrom.Enum => (false, false),
                DictionaryFrom.EnsureFunction => (false, false),
                _ => (true, true),
            };
        }

        #region 数据库字段
        [MaxLength(20), Required]
        public string Name
        {
            get; set;
        } = string.Empty;

        public DictionaryFrom From
        {
            get; set;
        } = DictionaryFrom.Database;

        public bool CanEdit
        {
            get; set;
        } = true;

        public bool CanDelete
        {
            get; set;
        } = true;

        public List<DictionaryEntry> Entries
        {
            get;
            set;
        } = [];
        #endregion

        public DictionaryEntry? this[string key] => Entries.FirstOrDefault(e => e.DataKey == key);
        public DictionaryEntry? this[Guid id] => Entries.FirstOrDefault(e => e.ID == id);

        internal readonly Dictionary<string, string> _dictionary = [];
        public Dictionary<string, string> ToDictionary()
        {
            if (_dictionary.Count > 0)
            {
                return _dictionary;
            }

            return ReBuild();
        }

        internal Dictionary<string, string> ReBuild()
        {
            foreach (var item in Entries)
            {
                if (item.Enable)
                {
                    _dictionary.Add(item.DataKey, item.DataValue);
                }
            }

            return _dictionary;
        }

        public void CreateEntries(params string[] keys)
        {
            foreach (var item in keys)
            {
                CreateEntry(item);
            }
        }

        /// <summary>
        /// 向字典中添加一个项，若键已经存在则修改键值
        /// <para>
        /// 该方法*不会*将项保存到数据库里！
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="order">顺序号，不输入则为当前最大顺序号加100</param>
        /// <param name="visible"></param>
        /// <param name="enable"></param>
        /// <returns>返回被添加/修改的对象</returns>
        public DictionaryEntry? CreateEntry(string key, int? order = null, bool enable = true, bool visiable = true)
        {
            order ??= Entries.Count > 0 ? Entries.Max(e => e.DisplayOrder) + 100 : 100;

            DictionaryEntry entry = new DictionaryEntry()
            {
                DataKey = key,
                DataValue = key,
                DataDictionaryId = ID,
                DisplayOrder = order.Value,
                Visible = visiable,
                Enable = enable,
                ID = Guid.Empty
            };

            return CreateEntry(entry);
        }

        /// <summary>
        /// 向字典中添加一个项，若键已经存在则修改键值
        /// <para>
        /// 该方法*不会*将项保存到数据库里！
        /// </para>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="order">顺序号，不输入则为当前最大顺序号加100</param>
        /// <param name="visible"></param>
        /// <param name="enable"></param>
        /// <returns>返回被添加/修改的对象</returns>
        public DictionaryEntry? CreateEntry(string key, string value, int? order = null, bool enable = true, bool visiable = true)
        {
            order ??= Entries.Count > 0 ? Entries.Max(e => e.DisplayOrder) + 100 : 100;

            DictionaryEntry entry = new DictionaryEntry()
            {
                DataKey = key,
                DataValue = string.IsNullOrWhiteSpace(value) ? key : value,
                DataDictionaryId = ID,
                DisplayOrder = order.Value,
                Visible = visiable,
                Enable = enable,
                ID = Guid.Empty
            };

            return CreateEntry(entry);
        }

        /// <summary>
        /// 向字典中添加一个项，若键已经存在则修改键值
        /// <para>
        /// 该方法*不会*将项保存到数据库里！
        /// </para>
        /// </summary>
        /// <returns>返回被添加/修改的对象</returns>
        public DictionaryEntry? CreateEntry(DictionaryEntry entry)
        {
            DictionaryEntry? exist = entry.ID == Guid.Empty ? this[entry.DataKey] : this[entry.ID];
            if (exist is not null)
            {
                // 已经存在这一项
                exist.CopyFromObject(entry);
            }
            else
            {
                entry.ID = entry.ID == Guid.Empty ? SequentialGuid.NewSuid() : entry.ID;
                entry.DataDictionaryId = ID;
                entry.SetPermission(From);

                Entries.Add(entry);
            }

            _dictionary.Clear();
            // Entries.Sort();
            return entry;
        }

        public DictionaryEntry? RemoveEntry(DictionaryEntry entry)
        {
            var ele = Entries.FirstOrDefault(e => e.ID == entry.ID);
            if (ele != null)
            {
                Entries.Remove(ele);
                _dictionary.Clear();
            }
            return ele;
        }

        /// <summary>
        /// 清除实体，但默认不清除canDelete属性为false的实体
        /// </summary>
        /// <param name="clearAll">设置为true时，清除所有实体</param>
        /// <returns>被清除的实体</returns>
        public List<DictionaryEntry> ClearEntries(bool clearAll = false)
        {
            List<DictionaryEntry> needRemove = [];
            if (clearAll)
            {
                needRemove = Entries.ToList();
                Entries.Clear();
            }
            else
            {
                needRemove = Entries.Where(e => e.CanDelete).ToList();
                Entries.RemoveAll(e => e.CanDelete);
            }

            return needRemove;
        }

        public FluentValidator BuildValidator()
        {
            FluentValidator<DataDictionary> fluent = new FluentValidator<DataDictionary>();
            fluent.AutoRulesByAttributes();

            fluent.RuleFor(e => e.From).Should((r, e) =>
            {
                (CanEdit, CanDelete) = From switch
                {
                    DictionaryFrom.Enum => (false, false),
                    DictionaryFrom.EnsureFunction => (true, false),
                    _ => (true, true),
                };
            });

            fluent.RuleFor(e => e.Name).Should((c, r, o) =>
            {
                DataDictionaryService dictService = (c.Resolve<IDataDictionaryService>() as DataDictionaryService)!;
                DataDictionary? dict = dictService[o.Name];

                if (dict != null && dict.ID != o.ID)
                {
                    r.SetFailure(nameof(Name), $"当前已经存在另一项名称为{o.Name}的字典，无法继续使用此名称。");
                }

                if (dict != null && !dict.CanEdit)
                {
                    r.SetFailure(nameof(Name), $"不允许修改名为{o.Name}的字典。");
                }
            });
            return fluent;
        }

        public override string ToString() => $"(字典：{Name})";

        public int CompareTo(DataDictionary? other)
        {
            if (other == null) return 1;
            return From != other.From ? (From < other.From ? 1 : -1) : (DisplayOrder > other.DisplayOrder ? 1 : -1);
        }
    }
}
