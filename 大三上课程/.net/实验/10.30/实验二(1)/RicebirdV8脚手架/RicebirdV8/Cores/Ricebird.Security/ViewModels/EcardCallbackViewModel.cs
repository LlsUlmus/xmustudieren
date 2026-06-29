namespace Ricebird.Security.ViewModels
{
    public class EcardCallbackViewModel
    {
        public bool IsMobile
        {
            get; set;
        } = false;

        public string Scence
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

        public string Domain
        {
            get; set;
        } = string.Empty;

        public string RedirectUrl
        {
            get; set;
        } = string.Empty;

        public AccessToken AccessToken
        {
            get; set;
        } = new (string.Empty, string.Empty);

        public EcardCallbackViewModel(IClient client, IUserPrincipal user, string token, string domain)
        {
            IsMobile = client.Browser.IsMobile;
            IsWechat = client.Browser.IsWechat;
            IsWorkWechat = client.Browser.IsWorkWechat;

            RealName = user.RealName;
            Code = user.Code;

            Token = token;
            Domain = domain;

            Scence = (IsMobile, IsWechat, IsWorkWechat) switch
            {
                (false, _, _) => "去往PC端主页",
                (true, false, false) => "去往移动端主页",
                (true, true, false) => "由微信进入，去往企业微信主页",
                (true, true, true) => "去往企业微信主页",
                _ => "未知场景",
            };
        }
    }
}
