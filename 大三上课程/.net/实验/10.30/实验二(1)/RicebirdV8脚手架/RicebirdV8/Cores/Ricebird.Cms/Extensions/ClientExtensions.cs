namespace Ricebird.Framework.Clients
{
    public static class ClientExtensions
    {
        public static (string uniqueCode, int page, int pageSize, bool hasContent) GetPaginationData(this IClient client)
        {
            string uniqueCode = client.Get(nameof(uniqueCode), string.Empty);
            int page = client.Get(nameof(page), 1);
            int pageSize = client.Get(nameof(pageSize), 10);
            bool hasContent = client.Get(nameof(hasContent), false);

            return (uniqueCode, page, pageSize, hasContent);
        }

        public static string GetPaginationKey(this IClient client)
        {
            var (uniqueCode, page, pageSize, hasContent) = client.GetPaginationData();
            return $"{INERNAL_CATEGORY_SOURCE}/{uniqueCode}/{page}/{pageSize}/{hasContent}";
        }
    }
}
