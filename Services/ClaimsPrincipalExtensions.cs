using System.Security.Claims;

namespace Todo_Api.Services;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("sub")
            ?? throw new InvalidOperationException("The token has no user id.");
        return int.Parse(value);
    }
}