using Microsoft.AspNetCore.SignalR;

namespace Ricebird.Sms.Hubs
{
    public class SignalRUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            string userId = connection.User?.FindFirst("ID")?.Value ?? string.Empty;
            return userId;
        }
    }
}
