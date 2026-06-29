using Microsoft.AspNetCore.RateLimiting;
using Ricebird.Framework.Clients;
using System.Threading.RateLimiting;

namespace Ricebird.Framework.AspNetCoreExtensions.RateLimiters
{
    public class LimitByUser : IRateLimiterPolicy<string>
    {
        public Func<OnRejectedContext, CancellationToken, ValueTask>? OnRejected => OnRequestRejected;

        public RateLimitPartition<string> GetPartition(HttpContext httpContext)
        {
            var client = httpContext.Features.Get<IClient>();
            if (client == null)
            {
                return RateLimitPartition.GetNoLimiter(string.Empty);
            }

            var userId = client.CurrentUser.ID;
            if (userId == Guid.Empty)
            {
                return RateLimitPartition.GetTokenBucketLimiter(client.RealIp.ToString(), _ =>
                {
                    return new TokenBucketRateLimiterOptions()
                    {
                        TokenLimit = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                        TokensPerPeriod = 10,
                        QueueLimit = 0,
                    };
                });
            }

            var limiter = RateLimitPartition.GetTokenBucketLimiter(userId.ToString(), _ =>
            {
                return new TokenBucketRateLimiterOptions()
                {
                    TokenLimit = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(60),
                    TokensPerPeriod = 10,
                    QueueLimit = 0,
                };
            });
            return limiter;
        }

        public async ValueTask OnRequestRejected(OnRejectedContext context, CancellationToken token)
        {
            context.HttpContext.Response.StatusCode = 200;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                success = false,
                msg = "访问频率过高，已经被限制流量"
            }, cancellationToken: token);
        }
    }
}
