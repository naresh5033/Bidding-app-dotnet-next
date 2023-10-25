using System.Security.Claims;

namespace AuctionService.IntegrationTests;

public class AuthHelper
{
    public static Dictionary<string, object> GetBearerForUser(string username)//so we can pass in diff user name to this method, and get the jwt and set the user name pass in to the username property
    {
        return new Dictionary<string, object>{{ClaimTypes.Name, username}};
    }
}
