
namespace Ricebird.Sms.Controllers
{
    public class SmsDebugController(ISmsService sender) : DebugController
    {
        [Route("~/toConn")]
        public ActionResult ToConn()
        {
            string connId = Get("conn", string.Empty);
            sender.SendToClient(connId, $"toConn Send", GenerateId(8));
            return Ok(connId);
        }

        [Route("~/toToken")]
        public ActionResult ToToken()
        {
            Guid userId = new Guid("8987534c-fa16-4f28-b2ce-c5a2ce82c7e4");
            sender.SendToUser(userId, $"ToToken Send", GenerateId(8));
            return Ok(userId);
        }
    }
}
