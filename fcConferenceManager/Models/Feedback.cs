using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace MAGI_API.Models
{

    public class FeedbackOpertions
    {   
        public async Task<List<FeedbackQuestion>> Feedback_question_select(string ActiveEventPkey, string Account_PKey)
        {
            try
            {
                int EventPkey = 0;
                int AccPKey = 0;

                int.TryParse(ActiveEventPkey, out EventPkey);
                int.TryParse(Account_PKey, out AccPKey);

                SqlParameter[] parameters = new SqlParameter[]
                {
                new SqlParameter("@Account_PKey", AccPKey),
                new SqlParameter("@ActiveEventPkey", EventPkey)
                };

                List<FeedbackQuestion> list = new List<FeedbackQuestion>();
                list = await SqlHelper.ExecuteListAsync<FeedbackQuestion>
                                                ("get_feedback_questions",
                                                CommandType.StoredProcedure, parameters);

                return list;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> Feedback_Submit(FeedBackList feedBackList)
        {
            try
            {
                string qry = "";
                bool Answerd = false;

                foreach (FeedbackPost fd in feedBackList.feedbackPosts)
                {
                    int resKey = fd.resKey;
                    string res = "";
                    string qPkey = fd.qPkey;
                    int Answer_pkey = fd.Answer_pkey;
                    int Form_pKey = fd.Form_pKey;
                    int FQ_pKey = fd.FQ_pKey;
                    Answerd = true;

                    switch (resKey)
                    {
                        case 1:
                            {
                                res = fd.res;
                                break;
                            }
                        case 2:
                            {

                                res = fd.res;
                                break;
                            }
                        case 3:
                            {
                                string x = "";
                                var pat = "[{*}]";
                                foreach (string str in fd.CheckBoxText)
                                {
                                    x += (x == "" ? "" : pat) + str.Trim();
                                }
                                res = x;
                                break;
                            }
                        case 4:
                            {
                                res = fd.res;
                                break;
                            }
                        case 5:
                            {
                                res = fd.res;
                                break;
                            }
                    }

                    if (Answer_pkey > 0)
                    {
                        qry = qry + Environment.NewLine + "Update EventFeedback_Response Set Response = '" + (res == null ? "" : res.Replace("'", "''")) + "'";
                        qry = qry + Environment.NewLine + "where pkey =" + Answer_pkey.ToString();
                        qry = qry + Environment.NewLine + " ";
                    }
                    else
                    {
                        qry = qry + Environment.NewLine + "insert into EventFeedback_Response (Account_pkey,Question_pkey,Response,[Date],Event_pKey,Forms_pKey,FQ_pKey)";
                        qry = qry + Environment.NewLine + "values(@acc," + qPkey.ToString() + ",'" + (res == null ? "" : res.Replace("'", "''")) + "',getdate(),@eve," + Form_pKey.ToString() + "," + FQ_pKey.ToString() + ")";
                        qry = qry + Environment.NewLine + " ";
                    }
                }

                if (Answerd)
                {
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                    new SqlParameter("@acc", feedBackList.intAccount_PKey.ToString()),
                    new SqlParameter("@eve", feedBackList.intActiveEventPkey)
                    };

                    await SqlHelper.ExecuteListAsync<FeedbackQuestion>
                                                (qry,
                                                CommandType.Text, parameters);
                }

                return Answerd;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class FeedbackPost
    {
        #region constructors
        public FeedbackPost()
        {
            CheckBoxText = new List<string>();
        }
        #endregion

        #region properties
        public int resKey { get; set; }
        public string res { get; set; }
        public string qPkey { get; set; }
        public int Answer_pkey { get; set; }
        public int Form_pKey { get; set; }
        public int FQ_pKey { get; set; }
        public List<string> CheckBoxText { get; set; }
        #endregion
    }

    public class FeedbackQuestion
    {
        public int pKey { get; set; }
        public int Forms_pKey { get; set; }
        public int FQ_pKey { get; set; }
        public string Question { get; set; }
        public int Res { get; set; }
        public string Reo { get; set; }
        public bool Req { get; set; }
        public int Answer_pkey { get; set; }
        public string Answer { get; set; }
        public string InstructionID { get; set; }
    }

    public class FeedBackList
    {
        #region constructors
        public FeedBackList()
        {
            feedbackPosts = new List<FeedbackPost>();
        }
        #endregion

        #region properties
        public List<FeedbackPost> feedbackPosts { get; set; }
        public string intAccount_PKey { get; set; }
        public string intActiveEventPkey { get; set; }
        #endregion
    }
}