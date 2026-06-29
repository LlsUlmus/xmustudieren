using Microsoft.AspNetCore.Mvc;

namespace Ricebird.Framework.FileStorage
{
    public abstract class FileApiController : ApiController
    {

        public new ActionResult File((string msg, IFile? file) ans)
        {
            if (ans.file == null)
            {
                return Fail(ans.msg);
            }

            return Ok(new
            {
                success = true,
                ans.msg,
                download = ans.file.DownloadPath,
                id = ans.file.UniqueCode
            });
        }
    }
}
