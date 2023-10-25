using System.Security.Claims;

namespace AuctionService.UnitTests;

public class Helpers
{
    public static ClaimsPrincipal GetClaimsPrincipal() //this security claims are the pieces of the info about the users and we need the user info for the testing
    {
        var claims = new List<Claim>{new Claim(ClaimTypes.Name, "test")};
        var identity = new ClaimsIdentity(claims, "testing");
        return new ClaimsPrincipal(identity);
    }
}
