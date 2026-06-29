namespace Ricebird.Configuration.Controllers
{
    [Route("~/api/config/[action]")]
    public class ConfigApiController(IOptionService oS) : ApiController
    {
        readonly IOptionService optionService = oS;

        public ActionResult GetConfigs()
        {
            var data = optionService.LoadOptions<WebOptions>();
            return RJson(new
            {
                success = true,
                msg = "",
                data
            });
        }
    }
}
