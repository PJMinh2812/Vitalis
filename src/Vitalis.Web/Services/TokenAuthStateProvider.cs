using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Vitalis.Web.Services;

// Builds the ClaimsPrincipal AuthorizeView/[Authorize] check against directly
// from AuthTokenStore.CurrentUser — no need to decode the JWT itself, since
// UserSummaryDto already carries the role list from the same login response.
public class TokenAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthTokenStore _tokenStore;

    public TokenAuthStateProvider(AuthTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
        _tokenStore.Changed += () => NotifyAuthenticationStateChanged(Task.FromResult(BuildState()));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(BuildState());

    private AuthenticationState BuildState()
    {
        if (!_tokenStore.IsAuthenticated || _tokenStore.CurrentUser is null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var user = _tokenStore.CurrentUser;
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            .. user.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        var identity = new ClaimsIdentity(claims, authenticationType: "ApiToken");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
