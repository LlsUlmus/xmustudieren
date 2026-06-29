namespace Ricebird.Framework.Database
{
    public record DatabaseDiagnostic(int TotalMilliseconds, int SqlCount, List<string> Logs);
}
