using System.Net.Http.Headers;

namespace Vitalis.Web.Services;

// Attached to the "VitalisApi" named HttpClient — every request server-side
// Blazor makes to the WebApi picks up the current circuit's access token
// automatically, so pages never have to set the header themselves.
public class AuthHeaderHandler(AuthTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (tokenStore.AccessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
