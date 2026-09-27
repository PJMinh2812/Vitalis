using Vitalis.Application.DTOs.Auth;

namespace Vitalis.Web.Services;

// Scoped = one instance per Blazor circuit (per connected browser tab), which
// is exactly the lifetime we want for "the current user's session". A hard
// page reload tears down the circuit and loses this — known simplification,
// no ProtectedBrowserStorage persistence yet (see learning-notes/11).
public class AuthTokenStore
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public UserSummaryDto? CurrentUser { get; private set; }

    public bool IsAuthenticated => AccessToken is not null && CurrentUser is not null;

    public event Action? Changed;

    public void SetSession(string accessToken, string refreshToken, UserSummaryDto user)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        CurrentUser = user;
        Changed?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;
        Changed?.Invoke();
    }
}
