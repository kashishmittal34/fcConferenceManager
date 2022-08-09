using MAGI_API.Models;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MAGI_API.Security
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            //If login request is for myBooth
            if (context.Parameters["appType"] != null && context.Parameters["appType"].ToString().ToUpper() == "BOOTHAPP")
            {
                SqlOperation o = new SqlOperation();
                bool res = await o.AllowMyboothUser(Convert.ToString(context.Parameters["username"]));
                if (!res)
                {
                    context.SetError("unauthorized");
                }
            }
            //return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //var allowedOrigin = "*";
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            SqlOperation ob = new SqlOperation();
            IdentityUser user = await ob.GetUserbyNameAndPassword(context.UserName, context.Password);
            if (user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
                return;
            }

            if (user.IntResult == 1)
            {
                context.SetError("invalid_grant", "The user name is incorrect");
                return;
            }
            else if (user.IntResult == 2)
            {
                context.SetError("invalid_grant", "The user password is incorrect");
                return;
            }
            else if (user.IntResult == 3)
            {
                context.SetError("invalid_grant", "This event is not yet open");
                return;
            }
            else if (user.IntResult == 4)
            {
                context.SetError("invalid_grant", "App login valid only for participants");
                return;
            }
            else if (user.IntResult == 5)
            {
                context.SetError("invalid_grant", "This event has closed");
                return;
            }
            if (!user.EmailConfirmed)
            {
                context.SetError("invalid_grant", "User did not confirm email");
                return;
            }
            var props = new AuthenticationProperties(new Dictionary<string, string>
            {
              {
                "userid", user.CustomerId
              },
              {
                "username", user.FirstName + " " + user.LastName
              },
              {
                "eventid", user.EventId
              },
              {
                "eventname", user.EventName
              },
              {
                "eventtypeid", user.EventTypeId
              },
              {
                "eventuserid", user.EventAccount_pkey
              },
              {
                "eventuserstatusid", user.ParticipationStatus_pKey
              },
              {
                "EventStartDate",Convert.ToDateTime( user.EventStartDate).ToString()
              },
              {
                "EventEndDate", Convert.ToDateTime(user.EventEndDate).ToString()
              },
              {
                "role", user.Roles.FirstOrDefault()
              },
              {
                "Region", user.Region.ToString()
              },
              {
                "RegionCode", user.RegionCode.ToString()
              },
              {
                "TimeOffset", user.TimeOffset.ToString()
              },
              {
                "ISEventFeedbackResponse",user.ISEventFeedbackResponse.ToString()
              },
              {
                 "LeftBlockImage", user.LeftBlockImage.ToString()
              }
                ,
              {
                 "LocationTimeInterval", user.LocationTimeInterval.ToString()
              },
                {
                    "RegistrationLevel_Pkey",user.RegistrationLevel_Pkey.ToString()
                },
                {
                    "IsLicenseNumber",(user.IsLicenseNumber.ToString())
                },
                 {
                    "IsSpeaker",(user.IsSpeaker.ToString())
                }
             });

            //var props = new AuthenticationProperties(new Dictionary<string, string>
            //{
            //  {
            //    "userid", System.Guid.NewGuid().ToString()
            //  },
            //  {
            //    "username", "API user"
            //  },
            //  {
            //    "role", "KeyUser"
            //  }
            // });
            var oAuthIdentity = new ClaimsIdentity("JWT");
            oAuthIdentity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
            oAuthIdentity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(oAuthIdentity));
            var ticket = new AuthenticationTicket(oAuthIdentity, props);
            context.Validated(ticket);

            //return Task.FromResult<object>(null);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }
    }
    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        readonly string _name;
        public QueryStringOAuthBearerProvider(string name)
        {
            _name = name;
        }
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var value = context.Request.Query.Get(_name);

            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }
            return Task.FromResult<object>(null);
        }
    }
}