
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
        return int.Parse(user.FindFirstValue("Id"));
    }

} }