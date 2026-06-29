using Ricebird.Framework.Security.Apis;

namespace Ricebird.Security.Controllers
{
    [Route("~/api/viewer/[action]"), ApiGroup("接口和权限管理")]
    public class ApiViewerController(ApiManager apiManager) : ApiController
    {
        [ApiShouldLogin("获取所有接口")]
        public ActionResult GetApiList()
        {
            apiManager.TryUpdateJson();
            return JsonString(apiManager.Json);
        }
    }
}
