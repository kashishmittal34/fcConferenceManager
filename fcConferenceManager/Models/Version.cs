using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace MAGI_API.Models
{
    public class VersionParameter
    {
        #region properties
        public int EventId { get; set; }
        public int AppType { get; set; }
        public string AccountId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceVersion { get; set; }
        public string DeviceInfo { get; set; }
        #endregion
    }

    public class Version
    {
        #region properties
        public string VersionNumber { get; set; }
        public string VersionText { get; set; }
        #endregion
    }

    #region Methods
    public class VersionOperations
    {
        public async Task<Version> getVersion(VersionParameter postSurvey)
        {
            try
            {
                string qry = "";
                if (postSurvey.AppType == 1)
                    qry += "select ms.[VersionNumberiOS] as VersionNumber, ms.[VersionTextiOS] as VersionText from Mobile_Settings ms";
                else
                    qry += "select ms.[VersionNumber], ms.[VersionText] from Mobile_Settings ms";
                qry += Environment.NewLine + "where ms.[Event_pkey] = @prmEventId";
                //qry += Environment.NewLine + "and ms.[AppType] = @prmAppType";
                if ((postSurvey.DeviceId.ToString() != "")) // && (postSurvey.DeviceToken != "")
                {
                    qry += Environment.NewLine + " EXEC Account_App_Token_Save @Account_pKey ,@prmEventId ,@prmAppType ,@DeviceID ,@DeviceToken ,@DeviceVersion";
                }
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@prmEventId", postSurvey.EventId),
                    new SqlParameter("@prmAppType", postSurvey.AppType),
                    new SqlParameter("@Account_pKey", postSurvey.AccountId),
                    new SqlParameter("@DeviceID", postSurvey.DeviceId),
                    new SqlParameter("@DeviceToken", postSurvey.DeviceToken),
                    new SqlParameter("@DeviceVersion", postSurvey.DeviceVersion),
          
              
                };

                Version version = await SqlHelper.ExecuteObjectAsync<Version>
                    (qry, CommandType.Text, parameters);

                return version;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
    #endregion
}