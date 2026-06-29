namespace Ricebird.Framework.Diagnostics
{
    public interface IMemoryLogger : IScopedDependency
    {
        List<string> Logs { get; }

        void Add(string msg);
    }
}
