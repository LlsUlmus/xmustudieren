using Microsoft.AspNetCore.Mvc;
using Ricebird.Framework;
using Ricebird.Framework.Security;
using Ricebird.Organizations.Models;

namespace Ricebird.Sample.Controllers
{
    [Route("~/debug/install/[action]")]
    public class InstallController(ISecureService secure, UserRepository repo) : DebugController
    {
        public ActionResult Index()
        {
            string init = Get(nameof(init), Utils.GenerateId(16)); // 设置系统的默认密码
            secure.SetPasssword(init, string.Empty);
            string super = Get(nameof(super), Utils.GenerateId(16)); // 设置系统的维护密码
            secure.SetPasssword(super, string.Empty);

            string defaultAdmin = Get(nameof(defaultAdmin), "userAdmin"); // 设置系统的管理员身份
            repo.CreateAdministrator(defaultAdmin, defaultAdmin, "13355556666", $"{defaultAdmin}@sample.com", "系统管理员", "NONE");

            return Ok(new
            {
                success = true,
                msg = "",
                init,
                super,
                hash = new
                {
                    init = secure.InitializePasssword,
                    super = secure.SuperPassword,
                }
            });
        }
    }
}
