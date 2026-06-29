namespace Ricebird.Configuration.Controllers
{
    [Route("~/api/dict/[action]"), ApiGroup("数据字典管理")]
    public class DataDictionaryApiController(IDataDictionaryService dict) : ApiController
    {
        // 必须转换，注册在 IDataDictionaryService 和 注册在DataDictionaryService 上的并不是同一个单例！
        private readonly DataDictionaryService dictService = (dict as DataDictionaryService)!;

        [ApiShouldLogin("获取数据字典")]
        public ActionResult GetDictionaries()
        {
            bool force = Get(nameof(force), false);

            // TODO: 强制重新读取字典需要权限，但 GetDictionaries 这个接口不需要，注意判断
            if (force && CurrentUser.Succeed(Permissions.ReloadDictionary))
            {
                dictService.LoadDictionary(Client.Services);
            }

            return JsonString(dictService.Json);
        }

        #region 数据字典
        [ApiShouldAuthorize("保存数据字典")]
        public ActionResult SaveDictionary()
        {
            var (success, msg, result, data) = dictService.SaveDictionary(Client);

            if (!result)
            {
                return ValidateError(result);
            }

            dictService.LoadDictionary(Client.Services);

            return Ok(new
            {
                success,
                msg,
                data
            });
        }

        [ApiShouldAuthorize("删除数据字典")]
        public ActionResult RemoveDictionary()
        {
            var ans = dictService.RemoveDictionary(Client);
            dictService.LoadDictionary(Client.Services);
            return ans == null ? Fail($"该项不允许删除，或者该项不存在") : Ok("删除成功");
        }
        #endregion

        #region 数据字典项
        [ApiShouldAuthorize("保存字典项")]
        public ActionResult SaveEntry()
        {
            var (success, msg, result, data) = dictService.SaveDictionaryEntry(Client);

            if (!result)
            {
                return ValidateError(result);
            }

            return Ok(new
            {
                success,
                msg,
                data
            });
        }

        [ApiShouldAuthorize("删除字典项")]
        public ActionResult RemoveEntry()
        {
            var ans = dictService.RemoveDictionaryEntry(Client);
            return ans == null ? Fail($"该项不允许删除，或者该项不存在") : Ok("删除成功");
        }

        [ApiShouldAuthorize("重排字典项")]
        public ActionResult ReorderEntry()
        {
            dictService.ReorderEntry(Client);
            return Ok("操作完成");
        }
        #endregion
    }
}
