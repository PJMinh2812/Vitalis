using System.Net.Http.Json;
using Vitalis.Application.DTOs.Auth;

namespace Vitalis.Web.Services;

// Thin wrapper over the named "VitalisApi" HttpClient so pages call
// `Api.LoginAsync(...)` instead of hand-rolling PostAsJsonAsync/error-parsing
// everywhere. Grows as more pages get wired — auth methods first.
public class VitalisApiClient(IHttpClientFactory httpClientFactory)
{
    private HttpClient Client => httpClientFactory.CreateClient("VitalisApi");

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<LoginRequest, AuthResponse>("api/auth/login", request, cancellationToken);

    public Task<AuthResponse> RegisterAsync(RegisterPatientRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<RegisterPatientRequest, AuthResponse>("api/auth/register", request, cancellationToken);

    public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        PostNoContentAsync("api/auth/logout", new RefreshTokenRequest { RefreshToken = refreshToken }, cancellationToken);

    public Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default) =>
        PostNoContentAsync("api/auth/forgot-password", request, cancellationToken);

    // --- Generic helpers, reused by every page's own API calls ---

    public async Task<TResponse> GetAsync<TResponse>(string url, CancellationToken cancellationToken = default)
    {
        var response = await Client.GetAsync(url, cancellationToken);
        return await ReadOrThrowAsync<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(url, body, cancellationToken);
        return await ReadOrThrowAsync<TResponse>(response, cancellationToken);
    }

    public async Task PostNoContentAsync<TRequest>(string url, TRequest body, CancellationToken cancellationToken = default)
    {
        var response = await Client.PostAsJsonAsync(url, body, cancellationToken);
        await ThrowIfNotSuccessAsync(response, cancellationToken);
    }

    public async Task<TResponse> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken cancellationToken = default)
    {
        var response = await Client.PutAsJsonAsync(url, body, cancellationToken);
        return await ReadOrThrowAsync<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse> PatchAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken cancellationToken = default)
    {
        var response = await Client.PatchAsJsonAsync(url, body, cancellationToken);
        return await ReadOrThrowAsync<TResponse>(response, cancellationToken);
    }

    public async Task PatchNoContentAsync<TRequest>(string url, TRequest body, CancellationToken cancellationToken = default)
    {
        var response = await Client.PatchAsJsonAsync(url, body, cancellationToken);
        await ThrowIfNotSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await ThrowIfNotSuccessAsync(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken))!;
    }

    private static async Task ThrowIfNotSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        string message = $"Lỗi {(int)response.StatusCode}";
        IDictionary<string, string[]>? errors = null;
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>(cancellationToken: cancellationToken);
            if (problem is not null)
            {
                message = problem.Detail ?? problem.Title ?? message;
                errors = problem.Errors;
            }
        }
        catch
        {
            // Response body wasn't ProblemDetails JSON — keep the generic message.
        }

        throw new ApiException((int)response.StatusCode, message, errors);
    }

    // Matches ExceptionHandlingMiddleware's ProblemDetails shape (Title/Detail/Extensions["errors"]).
    private record ProblemDetailsBody(string? Title, string? Detail, IDictionary<string, string[]>? Errors);
}
