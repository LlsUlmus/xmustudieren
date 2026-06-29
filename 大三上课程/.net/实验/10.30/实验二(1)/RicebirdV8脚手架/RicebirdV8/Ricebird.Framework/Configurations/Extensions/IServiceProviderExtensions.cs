namespace System
{
    public static class IServiceProviderExtensionsInOptionService
    {
        public static T LoadOptions<T>(this IServiceProvider provider) where T : IOption, new()
        {
            IOptionService optService = provider.Resolve<IOptionService>();
            return optService.LoadOptions<T>();
        }
    }
}
