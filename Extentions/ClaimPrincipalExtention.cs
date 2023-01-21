
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace API.Extensions { 
public static class ClaimPrincipalExtention
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
            // return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usrName = user.Claims.FirstOrDefault(f => f.Type.Equals("name"));
            if (usrName != null)
                return usrName.Value;
            else
                return "";
        }

    public static int GetUserId(this ClaimsPrincipal user)
    {
            var userClaimId = user.Claims.FirstOrDefault(f => f.Type.Equals("sid"));
            if (userClaimId != null)
                return int.Parse(userClaimId.Value);
            else
                return 0;
           
    }

} }