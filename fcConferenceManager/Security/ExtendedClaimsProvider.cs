using System.Collections.Generic;
using System.Security.Claims;

namespace MAGI_API.Security
{
    public static class ExtendedClaimsProvider
    {
        public static IEnumerable<Claim> GetClaims(IdentityUser user)
        {
            List<Claim> claims = new List<Claim>();
            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            if (user.Claims != null)
            {
                foreach (var claim in user.Claims)
                {
                    claims.Add(claim);                    
                }
            }

            //var daysInWork = (DateTime.Now.Date - user.JoinDate).TotalDays;
            //if (daysInWork > 90)
            //{
            //    claims.Add(CreateClaim("FTE", "1"));
            //}
            //else
            //{
            //    claims.Add(CreateClaim("FTE", "0"));
            //}

            return claims;
        }

        public static Claim CreateClaim(string type, string value)
        {
            return new Claim(type, value, ClaimValueTypes.String);
        }
    }
}