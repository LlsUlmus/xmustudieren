namespace Ricebird.Framework.Configurations
{
    public interface IOptionStore : IDependency
    {
        public T LoadOptions<T>(T option)
            where T : IOption, new();

        public void SaveOptions(IOption opt);

        public OptionSaveTo SaveTo { get; }
    }
}
