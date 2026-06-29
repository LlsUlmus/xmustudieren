namespace Ricebird.Framework.Tools.ValueConverter
{
    public interface IConverter : IDependency
    {
        public int Order { get; }
        public bool IsSuit(Type toType, object fromValue);
        public object Convert(object fromValue, Type toType, object defaultValue);
    }
}
