namespace Ricebird.Wechat.ViewModels
{
    public class WechatLoginCallbackViewModel
    {
        public string ToUrl
        {
            get; set;
        } = string.Empty;

        public bool IsWechat
        {
            get; set;
        } = false;

        public bool IsWorkWechat
        {
            get; set;
        } = false;

        public string RealName
        {
            get; set;
        } = string.Empty;

        public string Code
        {
            get; set;
        } = string.Empty;

        public string Token
        {
            get; set;
        } = string.Empty;

        public WechatLoginCallbackViewModel(IClient client, IUserPrincipal user, string token, string toUrl)
        {
            IsWechat = client.Browser.IsWechat;
            IsWorkWechat = client.Browser.IsWorkWechat;

            RealName = user.RealName;
            Code = user.Code;

            Token = token;

            ToUrl = toUrl.Contains('?') ? $"{toUrl}&{ConstKeys.AuthenticationKey}={token}" : $"{toUrl}?{ConstKeys.AuthenticationKey}={token}";
        }
    }
}
