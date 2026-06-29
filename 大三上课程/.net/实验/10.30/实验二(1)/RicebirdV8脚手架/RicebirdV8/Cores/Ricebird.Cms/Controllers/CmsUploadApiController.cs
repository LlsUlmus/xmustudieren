using Ricebird.Framework.FileStorage;

namespace Ricebird.Cms.Controllers
{
    [Route("~/api/cms/upload/{action}")]
    public class CmsUploadApiController(IFileStorageService ifs) : FileApiController
    {
        private readonly IFileStorageService fileStorage = ifs;

        [ApiShouldLogin("上传文件")]
        public ActionResult File()
        {
            if (Request.Form.Files.Count != 1)
            {
                return Ok(new
                {
                    success = false,
                    msg = "上传文件必须有且只有1个"
                });
            }

            var file = Request.Form.Files[0];
            string srcFileName = file.FileName;
            string moduleName = Get("module", MODULE_NAME);
            var ans = fileStorage.CreateFile(file.OpenReadStream(), srcFileName, moduleName, Client);

            return File(ans);
        }

        [ApiShouldLogin("按BASE64格式上传文件")]
        public ActionResult Base64()
        {
            string base64 = Get("base64", string.Empty);
            string srcFileName = Get("file", string.Empty);
            string moduleName = Get("module", MODULE_NAME);
            var ans = fileStorage.CreateFile(base64, srcFileName, moduleName, Client);

            return File(ans);
        }
    }
}
