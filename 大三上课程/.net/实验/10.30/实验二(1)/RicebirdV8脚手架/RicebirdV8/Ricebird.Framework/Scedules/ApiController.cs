using Microsoft.AspNetCore.Mvc;
using Ricebird.Framework.Scedules;

namespace Ricebird.Framework
{
    public partial class ApiController
    {
        protected ActionResult Schedule(RicebirdSchedule schedule) => Ok(new
        {
            success = true,
            msg = "",
            id = schedule.ID
        });
    }
}
