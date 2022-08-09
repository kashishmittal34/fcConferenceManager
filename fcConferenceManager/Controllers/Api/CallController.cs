using MAGI_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MAGI_API.Controllers
{
    [RoutePrefix("api/Call")]
    public class CallController : ApiController
    {
        static SqlOperation repository = new SqlOperation();       

        [HttpPost]
        [Route("ADDCall")]
        public async Task<string> ADDCall(CallRecording Call)
        {           
            var response = await repository.Call_Recording_Save (Call);
            return response;
        }


        [HttpPost]
        [Route("JournalSubscription/{Account_pkey}/{status}")]
        public async Task<string> JournalSubscription(string Account_pkey , int status )
        {
            var response = await repository.JournalSubscriptionSave(Account_pkey, status);
            return response;
        }


        [HttpPost]
        [Route("TempRecord")]
        public async Task<string> TempRecord(CallRecording Call)
        {
            var response = await repository.Temp_log_save(Call);
            return response;
        }

        [HttpPost]
        [Route("VirtualExibitorList")]
        public async Task<List<Exibitor>> VirtualExibitorList(CallRecording Call)
        {            
            List<Exibitor> li = await repository.VirtualExibitorList(Call);
            return li;
        }

        [HttpPost]
        [Route("CallList")]
        public async Task<List<Call_List>> CallList(CallRecording Call)
        {
            List<Call_List> li = await repository.Call_recording_select(Call);
            return li;
        }        

        [HttpPost]
        [Route("VerifyURL")]
        public async Task<bool> VerifyURL([FromBody] VerifyURL URL)
        {
            bool response = await repository.IsAllUrlVerify(URL.URL);
            return response;
        }

        [HttpPost]
        [Route("IssueAdd")]
        public async Task<int> IssueAdd([FromBody] IssueItem issueItem)
        {
            var response = await repository.Call_IssueList_Add(issueItem);
            return response;
        }

        [HttpPost]
        [Route("IssueSave")]
        public async Task<int> IssueSave([FromBody] IssueItem issueItem)
        {            
            var response = await repository.Call_IssueList_Save(issueItem);
            return response;
        }

        [HttpPost]
        [Route("IssueUpdate")]
        public async Task<int> IssueUpdate([FromBody] IssueItem issueItem)
        {
            var response = await repository.Call_IssueList_Update(issueItem);
            return response;
        }

        [HttpPost]
        [Route("IssueList")]
        public async Task<List<IssueItem_List>> IssueList(IssueItem Call)
        {
            List<IssueItem_List> li = await repository.Issue_item_select(Call);
            return li;
        }

        [HttpPost]
        [Route("IssuePage")]
        public async Task<List<IssueItem_List>> IssuePage(IssueItem Call)
        {
            List<IssueItem_List> li = await repository.Issue_page_select(Call);
            return li;
        }

        [HttpPost]
        [Route("IssueExcel")]
        public async Task<List<IssueItem_List>> IssueExcel(IssueItem Call)
        {
            List<IssueItem_List> li = await repository.Issueitem_select(Call);
            return li;
        }

        [HttpPost]
        [Route("UpdatePostLike")]
        public async Task<string> UpdatePostLike(LikeUpdate Post)
        {
            return await new SqlOperation().UpdatePost_Likes(Post.PostID, Post.AccountID);
        }
    }
}