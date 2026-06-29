using UEditor.Services;

namespace Ricebird.Cms.Controllers
{
    [Route("~/api/ueditor/{action}"), ApiGroup("新闻管理")]
    public class UEditorController : ApiController
    {
        [HttpGet, HttpPost, ApiShouldLogin("UEditor操作")]
        public ActionResult Process()
        {
            string action = Get("action", string.Empty);
            string jsonpCallback = Get("callback", string.Empty);
            string json = string.Empty;
            try
            {
                var handler = Client.Resolve<IUEditorHandler>(action);
                if (handler != null)
                {
                    json = handler.Process();
                }
                if (string.IsNullOrWhiteSpace(jsonpCallback))
                {
                    return Content(json);
                }
                else
                {
                    return JsonP(json);
                }
            }
            catch (NotSupportedException ex) when (ex.Message.Contains("找不到名为"))
            {
                return Fail(ex.Message);
            }
        }
    }
}
