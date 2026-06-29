namespace Ricebird.Framework.SignalR
{
    public interface ISmsService : ISingletonDependency
    {
        #region 向客户端发送消息
        void SendToClient(string connId, params object[] args);
        void SendToClient(string connId, string method, params object[] args);
        Task SendToClientAsync(string connId, params object[] args);
        Task SendToClientAsync(string connId, string method, params object[] args);
        void SendToUser(Guid userId, params object[] args);
        void SendToUser(Guid userId, string method, params object[] args);
        Task SendToUserAsync(Guid userId, params object[] args);
        Task SendToUserAsync(Guid userId, string method, params object[] args);
        #endregion
    }
}
