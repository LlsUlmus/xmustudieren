using Microsoft.AspNetCore.SignalR;

namespace Ricebird.Sms.Hubs
{
    public class RicebirdHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            if (!Context.User?.Identity?.IsAuthenticated ?? false)
            {
                Context.Abort();
                return;
            }

            string token = Context.User?.FindFirst("Token")?.Value ?? string.Empty;
            Context.Items.Add("token", token);

            await base.OnConnectedAsync();
        }
    }
}
