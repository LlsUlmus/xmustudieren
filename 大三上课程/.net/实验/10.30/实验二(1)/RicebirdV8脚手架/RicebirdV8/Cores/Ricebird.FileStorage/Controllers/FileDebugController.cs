namespace Ricebird.FileStorage.Controllers
{
    [Route("~/debug/file/{action}")]
    public class FileDebugController(MimeTypeService mts) : DebugController
    {
        public ActionResult GetMime()
        {
            return Ok(new
            {
                success = true,
                msg = "",
                data = mts.MimeTypes
            });
        }
    }
}
