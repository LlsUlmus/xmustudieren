using Microsoft.AspNetCore.SignalR;
using Ricebird.Sms.Hubs;

namespace Ricebird.Sms.Services
{
    internal class SmsService(IHubContext<RicebirdHub> hub) : ISmsService
    {
        private const string DEFAULT_METHOD = "to-client";

        public async Task SendToClientAsync(string connId, params object[] args)
        {
            await SendToClientAsync(connId, DEFAULT_METHOD, args);
        }

        public void SendToClient(string connId, params object[] args)
        {
            SendToClient(connId, DEFAULT_METHOD, args);
        }

        public async Task SendToClientAsync(string connId, string method, params object[] args)
        {
            await hub.Clients.Client(connId).SendAsync(method, args);
        }

        public void SendToClient(string connId, string method, params object[] args)
        {
            SendToClientAsync(connId, method, args).GetAwaiter().GetResult();
        }

        public async Task SendToUserAsync(Guid userId, params object[] args)
        {
            await SendToUserAsync(userId, DEFAULT_METHOD, args);
        }

        public void SendToUser(Guid userId, params object[] args)
        {
            SendToUser(userId, DEFAULT_METHOD, args);
        }

        public async Task SendToUserAsync(Guid userId, string method, params object[] args)
        {
            await hub.Clients.User(userId.ToString()).SendAsync(method, args);
        }

        public void SendToUser(Guid userId, string method, params object[] args)
        {
            SendToUserAsync(userId, method, args).GetAwaiter().GetResult();
        }
    }
}
