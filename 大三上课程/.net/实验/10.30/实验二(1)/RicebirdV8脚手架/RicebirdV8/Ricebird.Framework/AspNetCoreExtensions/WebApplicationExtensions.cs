namespace Microsoft.AspNetCore.Builder
{
    public static class WebApplicationExtensions
    {
        public static void UseRicebirdModules(this WebApplication app)
        {
            var env = app.Services.Resolve<HostEnv>();
            env.UseRicebirdModules(app);
        }
    }
}
