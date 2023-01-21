
using System.Linq;
using System.Security.Claims;
namespace API.Extensions { 
public static class ClaimPrincipalExtention
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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