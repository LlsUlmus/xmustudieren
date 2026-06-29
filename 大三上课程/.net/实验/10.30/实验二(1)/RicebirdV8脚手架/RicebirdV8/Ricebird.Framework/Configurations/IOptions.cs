namespace Ricebird.Framework.Configurations
{
    public interface IOption
    {
        [JsonIgnore]
        OptionSaveTo OptionSaveTo { get; }

        [JsonIgnore]
        string SaveKey { get; }

    }
}
