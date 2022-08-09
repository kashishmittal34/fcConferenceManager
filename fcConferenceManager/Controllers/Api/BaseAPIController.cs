using MAGI_API.Security;
using System.Web.Http;

namespace MAGI_API.Controllers
{
    public class BaseApiController : ApiController
    {       

        public BaseApiController()
        {
            
        }

        //private UserOperation _modelUser;    
        //protected UserOperation modelUser
        //{
        //    get
        //    {
        //        if (_modelUser == null)
        //        {
        //            _modelUser = new UserOperation(this.Request);
        //        }
        //        return _modelUser;
        //    }
        //}        

        protected IHttpActionResult GetErrorResult(Result result)
        {
            if (result == null)
            {
                return InternalServerError();
            }
            if (!result.Succeeded)
            {
                if (!string.IsNullOrEmpty(result.Error))
                {
                    ModelState.AddModelError("ErrorMessage", result.Error);
                }
                if (ModelState.IsValid)
                {                   
                    return BadRequest();
                }
                return BadRequest(ModelState);
            }
            return null;
        }
        
    }
}