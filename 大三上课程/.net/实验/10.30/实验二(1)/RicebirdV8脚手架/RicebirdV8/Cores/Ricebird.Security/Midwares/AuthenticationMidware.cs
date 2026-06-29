using Ricebird.Framework.AspNetCoreExtensions;

namespace Ricebird.Security.Midwares
{
    internal class AuthenticationMidware(RequestDelegate next) : RicebirdMidware(next)
    {
        public override async Task Invoke(HttpContext context)
        {
            SecurityService authService = context.Resolve<SecurityService>();
            var client = EnsureInitialize<IClient>(context, "身份验证服务", "Ricebird.Clients");

            IUserPrincipal? user = null;
            string token = client.GetInRequest(ConstKeys.AuthenticationKey, string.Empty);
            if (!token.HasValue())
            {
                token = client.Request?.Cookies[ConstKeys.AuthenticationKey]?.ToString() ?? string.Empty;
            }

            if (token.HasValue())
            {
                user = authService.GetUserPrinciple(token);
            }

            client.Type = user == null ? ClientType.Anonymous : ClientType.SignIn;
            user ??= authService.Anonymous;
            context.User = user.GetClaimsPrincipal(token);
            client.Features.Set<IUserPrincipal>(user);
            client.Features.Set(new AccessToken(ConstKeys.AuthenticationKey, token));
            await _next(context);
        }
    }
}
