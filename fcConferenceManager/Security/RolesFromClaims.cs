using System.Collections.Generic;
using System.Security.Claims;

namespace MAGI_API.Security
{
    public class RolesFromClaims
    {
        public static IEnumerable<Claim> CreateRolesBasedOnClaims(ClaimsIdentity identity)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Role, "KeyUser"));
            return claims;
        }
    }
}