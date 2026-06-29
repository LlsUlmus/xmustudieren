using Ricebird.Scedules.Services;

namespace Ricebird.Scedules.Controllers
{
    [Route("~/api/schedules/[action]"), ApiGroup("任务管理")]
    public class ScheduleApiController(IScheduleService sS) : ApiController
    {
        // 必须转换，注册在 IScheduleService 和 注册在ScheduleService 上的并不是同一个单例！
        private readonly ScheduleService sService = (sS as ScheduleService)!;

        [ApiShouldLogin("获取任务列表")]
        public ActionResult GetSchedules()
        {
            var (success, msg, data) = sService.GetSchedules(Client.CurrentUser.ID);
            return Ok(new
            {
                success,
                msg,
                data
            });
        }
    }
}
