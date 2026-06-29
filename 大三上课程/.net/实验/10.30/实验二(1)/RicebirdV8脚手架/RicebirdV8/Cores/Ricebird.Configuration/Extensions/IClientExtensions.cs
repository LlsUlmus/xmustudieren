namespace Ricebird.Framework.Clients
{
    public static class IClientExtensions
    {
        public static DataDictionary EnsureCreateDataDictionary(this IClient client, string dictName, Action<DataDictionary> entryBuilder)
        {
            DataDictionaryService dictService = client.Resolve<DataDictionaryService>();
            return dictService.EnsureCreate(client, dictName, entryBuilder);
        }

        public static DataDictionary EnsureCreateDataDictionary(this IClient client, string dictName)
        {
            return EnsureCreateDataDictionary(client, dictName, _ => { });
        }
    }
}
