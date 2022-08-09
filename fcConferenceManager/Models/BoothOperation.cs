using System.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MAGI_API.Security;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Net;
using System.Linq;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using fcConferenceManager;
using System.Globalization;
using static MAGI_API.Models.BoothValidate;

namespace MAGI_API.Models
{
    public class BoothOperation
    {
        public async Task<List<BoothList>> Booth_item_select(string Event_pkey)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                  new SqlParameter("@Event_pkey", Event_pkey)
            };
            List<BoothList> list = await SqlHelper.ExecuteListAsync<BoothList>("BoothAPI_BoothSetting_Select", CommandType.StoredProcedure, parameters);//Issueitem_select
            return list;
        }
    }
}