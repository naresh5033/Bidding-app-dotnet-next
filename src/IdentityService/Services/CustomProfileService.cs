using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService;

// to create a profile class and we can add it to the jwt (so we can get the user profile information)
public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject); // is the user id
        var existingClaims = await _userManager.GetClaimsAsync(user);   

        var claims = new List<Claim>
        {
            new Claim("username", user.UserName) //add the username
        };

        context.IssuedClaims.AddRange(claims); // we re adding 2 claims user name and the user full name to the jwt
        context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)); // here the user full name
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}
