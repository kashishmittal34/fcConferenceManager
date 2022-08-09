using fcConferenceManager.Models;
using MAGI_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;


namespace fcConferenceManager.Controllers
{
    [RoutePrefix("api/Partner")]
    public class PartnerStatusController : ApiController
    {
        static SqlOperation repository = new SqlOperation();

        [HttpPost]
        [Route("PartnerStatus")]
        public async Task<int> PartnerStatus(PartnerEventStatus Call)
        {
            var response = await repository.PartnerStatus_Update(Call);
            return response;
        }
    }
}