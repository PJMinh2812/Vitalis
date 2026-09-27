using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Vitalis.Application.DTOs.Auth;

namespace Vitalis.Web.Services;

// Attached to the "VitalisApi" named HttpClient — every request server-side
// Blazor makes to the WebApi picks up the current circuit's access token
// automatically, so pages never have to set the header themselves. The
// access token only lives 15 minutes (Jwt:AccessTokenMinutes) — a 401
// triggers one refresh-and-retry using AuthTokenStore's refresh token before
// giving up, so a long-lived circuit doesn't start failing every call.
public class AuthHeaderHandler(AuthTokenStore tokenStore, IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        byte[]? contentBytes = null;
        string? contentType = null;
        if (request.Content is not null)
        {
            contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            contentType = request.Content.Headers.ContentType?.ToString();
        }

        if (tokenStore.AccessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || tokenStore.RefreshToken is null)
            return response;

        if (!await TryRefreshAsync(cancellationToken))
        {
            tokenStore.Clear();
            return response;
        }

        response.Dispose();
        var retryRequest = CloneRequest(request, contentBytes, contentType);
        retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenStore.AccessToken);
        return await base.SendAsync(retryRequest, cancellationToken);
    }

    private async Task<bool> TryRefreshAsync(CancellationToken cancellationToken)
    {
        if (tokenStore.RefreshToken is not { } refreshToken)
            return false;

        try
        {
            var client = httpClientFactory.CreateClient("VitalisApiRaw");
            var response = await client.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest { RefreshToken = refreshToken }, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return false;

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: cancellationToken);
            if (auth is null)
                return false;

            tokenStore.SetSession(auth.AccessToken, auth.RefreshToken, auth.User);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original, byte[]? contentBytes, string? contentType)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);
        foreach (var header in original.Headers)
        {
            if (header.Key == "Authorization")
                continue;
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (contentBytes is not null)
        {
            clone.Content = new ByteArrayContent(contentBytes);
            if (contentType is not null)
                clone.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);
        }

        return clone;
    }
}
