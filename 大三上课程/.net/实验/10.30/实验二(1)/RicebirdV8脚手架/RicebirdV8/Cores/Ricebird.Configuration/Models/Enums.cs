namespace Ricebird.Configuration.Models
{
    [DataDictionary("字典来源")]
    public enum DictionaryFrom : int
    {
        [DataEntry("枚举")]
        Enum = 0,
        [DataEntry("内置")]
        EnsureFunction = 1,
        [DataEntry("自建")]
        Database = 2,
    }
}
