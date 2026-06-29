namespace Ricebird.Framework.Configurations
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class DataDictionaryAttribute(string name) : Attribute
    {
        public DataDictionaryAttribute() : this("ALL") { }

        public string Name { get; set; } = name;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class DataEntryAttribute(string name, int order) : Attribute
    {
        public DataEntryAttribute(string name) : this(name, -1)
        {

        }

        public string Value { get; set; } = name;

        public string Key { get; set; } = string.Empty;

        public bool Visible { get; set; } = true;

        public bool Enable { get; set; } = true;
        public int Order { get; set; } = order;
    }

    public interface IDataDictionaryService : ISingletonDependency
    {
        Dictionary<string, string>? GetDictionary(string name);
        Dictionary<string, string> GetRequiredDictionary(string name);
    }
}
