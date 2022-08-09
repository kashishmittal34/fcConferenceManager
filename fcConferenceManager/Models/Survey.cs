using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace MAGI_API.Models
{
    public class SurveyOpertions
    {
        public async Task<SurveyQuestionList> Survey_question_select(string accountKey, string EventKey, string SurveyKey)
        {
            try
            {
                int AccPKey = 0;
                int EveKey = 0;
                int sKey = 0;

                int.TryParse(accountKey, out AccPKey);
                int.TryParse(EventKey, out EveKey);
                int.TryParse(SurveyKey, out sKey);

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@intAccount_PKey", AccPKey),
                    new SqlParameter("@intCurEventPKey", EventKey),
                    new SqlParameter("@intExhibitorFeedbackFormPkey", sKey)
                };

                SurveyQuestionList surveyQuestionList = new SurveyQuestionList();
                surveyQuestionList.surveyQuestions = await SqlHelper.ExecuteListAsync<SurveyQuestion>
                                                ("get_SurveyQuestions",
                                                CommandType.StoredProcedure, parameters);
                surveyQuestionList.EventOrganizationPkey = await getEventOrganization_pkey(AccPKey.ToString(), EventKey.ToString());

                return surveyQuestionList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Boolean> Post_Survey(PostSurvey postSurvey)
        {
            try
            {
                string qry = "";
                bool Answerd = false;

                foreach (SurveyAnswer sa in postSurvey.surveyAnswers)
                {
                    string res = "";
                    int resKey = sa.resKey;
                    string qPkey = sa.qPkey;
                    int Answer_pkey = sa.Answer_pkey;
                    Answerd = true;

                    if (resKey == 1 || resKey == 2 || resKey == 4 || resKey == 5)
                    {
                        if (sa.res != "")
                            res = sa.res;
                    }
                    else if (resKey == 3)
                    {
                        string x = "";
                        var pat = "[{*}]";
                        foreach (string str in sa.CheckBoxText)
                        {
                            x += (x == "" ? "" : pat) + str;
                        }
                        res = x;
                    }
                    //tempfix start
                    int[] intArray = { 145, 146, 147, 148, 150, 151 };
                    if (intArray.Contains(Answer_pkey))
                    {
                        postSurvey.intExhibitorFeedbackFormPkey = 12;
                    }
                    //tempfix end
                    if (Answer_pkey > 0)
                    {
                        qry = qry + Environment.NewLine + "Update AttendeeSurvey_Response Set Response = '" + (res == null ? "" : res.Replace("'", "''")) + "'";
                        qry = qry + Environment.NewLine + "where pkey = " + Answer_pkey.ToString();
                        qry = qry + Environment.NewLine + " ";
                    }
                    else
                    {
                        qry = qry + Environment.NewLine + "Insert Into AttendeeSurvey_Response(Account_pkey,Question_pkey,Response,ResponseDate,Event_pKey,ExhibitorFeedbackForm_pkey,EventOrganization_pkey)";
                        qry = qry + Environment.NewLine + "Values(@acc," + qPkey.ToString() + ",'" + (res == null ? "" : res.Replace("'", "''")) + "',getdate(),@eve,'" + postSurvey.intExhibitorFeedbackFormPkey + "',@EventOrganization_pkey)";
                        qry = qry + Environment.NewLine + " ";
                    }
                }

                if (Answerd)
                {
                    SqlCommand cmd = new SqlCommand(qry);
                    cmd.Parameters.AddWithValue("@acc", postSurvey.intAccount_PKey.ToString());
                    cmd.Parameters.AddWithValue("@eve", postSurvey.intCurEventPKey.ToString());
                    cmd.Parameters.AddWithValue("@EventOrganization_pkey", (postSurvey.intExhibitorFeedbackFormPkey == 7 ? postSurvey.intEventOrganizationPkey : 0));

                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@acc", postSurvey.intAccount_PKey.ToString()),
                        new SqlParameter("@eve", postSurvey.intCurEventPKey.ToString()),
                        new SqlParameter("@EventOrganization_pkey", (postSurvey.intExhibitorFeedbackFormPkey == 7 ? postSurvey.intEventOrganizationPkey : 0))
                    };

                    await SqlHelper.ExecuteListAsync<FeedbackQuestion>
                                                (qry,
                                                CommandType.Text, parameters);
                }

                return Answerd;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> getEventOrganization_pkey(string AccPKey, string EventKey)
        {
            int key = 0;

            string qry = "Select Isnull(t1.pkey,0) pKey";
            qry = qry + Environment.NewLine + "From Event_Organizations t1";
            qry = qry + Environment.NewLine + "inner join Organization_List t2 on t1.Organization_pKey = t2.pkey";
            qry = qry + Environment.NewLine + "left join Account_List t3 on t3.ParentOrganization_pkey = t2.pkey";
            qry = qry + Environment.NewLine + "where t3.pkey=@Account_Pkey";
            qry = qry + Environment.NewLine + "and t1.Event_pkey = @Event_pkey";

            SqlCommand cmd = new SqlCommand(qry);
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Account_Pkey", AccPKey),
                new SqlParameter("@Event_pkey", EventKey),
            };
            key = SqlHelper.ExecuteScaler(qry, CommandType.Text, parameters);

            return key;
        }
    }

    public class PostSurvey
    {
        #region Constructor
        public PostSurvey()
        {
            surveyAnswers = new List<SurveyAnswer>();
        }
        #endregion

        #region Properties
        public List<SurveyAnswer> surveyAnswers { get; set; }
        public int intExhibitorFeedbackFormPkey { get; set; }
        public int intAccount_PKey { get; set; }
        public int intCurEventPKey { get; set; }
        public int intEventOrganizationPkey { get; set; }
        #endregion
    }

    public class SurveyAnswer
    {
        #region Constructor
        public SurveyAnswer()
        {
            CheckBoxText = new List<string>();
        }
        #endregion

        #region Properties
        public int resKey { get; set; }
        public string res { get; set; }
        public string qPkey { get; set; }
        public int Answer_pkey { get; set; }
        public List<string> CheckBoxText { get; set; }
        #endregion
    }
    
    public class SurveyQuestionList
    {
        public SurveyQuestionList()
        {
            surveyQuestions = new List<SurveyQuestion>();
        }

        public List<SurveyQuestion> surveyQuestions { get; set; }
        public int EventOrganizationPkey { get; set; }
    }

    public class SurveyQuestion
    {
        #region Properties
        public int pKey { get; set; }
        public string Question { get; set; }
        public string Reo { get; set; }
        public int Res { get; set; }
        public string Answer { get; set; }
        public int Answer_pkey { get; set; }
        #endregion
    }
}