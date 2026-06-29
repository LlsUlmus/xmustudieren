namespace Ricebird.Security.Controllers
{
    [Route("/debug/security/[action]")]
    public class SecurityDebugController(SecurityService securityService, ISecureService secure) : DebugController
    {
        public ActionResult GetToken()
        {
            string loginToken = Get("token", "adminUser");
            (bool success, string msg, string token, IUserPrincipal data) = securityService.GetCredential(loginToken, Client);

            if (success)
            {
                Client.Logger.User.SignInUser(Client, data);
                Response.Cookies.Append(ConstKeys.AuthenticationKey, token);
            }

            return Ok(new
            {
                success,
                msg,
                token,
                data
            });
        }

        public ActionResult SetInitializePassword()
        {
            string pwd = Get(nameof(pwd), GenerateId(16));
            secure.SetPasssword(pwd, string.Empty);
            return Ok(new
            {
                success = true,
                msg = "",
                pwd,
                hash = secure.InitializePasssword
            });
        }

        public ActionResult SetSuperPassword()
        {
            string pwd = Get(nameof(pwd), GenerateId(16));
            secure.SetPasssword(string.Empty, pwd);
            return Ok(new
            {
                success = true,
                msg = "",
                pwd,
                hash = secure.SuperPassword
            });
        }
    }
}
