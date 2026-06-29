namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        public static T Resolve<T>(this HttpContext context)
            where T : class => context.RequestServices.Resolve<T>();
    }
}
