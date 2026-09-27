using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Vitalis.Web.Components;
using Vitalis.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth: Vitalis.Web never signs this cookie scheme in — the real session lives
// in AuthTokenStore/TokenAuthStateProvider. It's registered only so a direct
// HTTP request to an [Authorize] page with no active circuit (fresh tab, hard
// refresh) has an IAuthenticationService to challenge against; the challenge
// just redirects to /login instead of crashing, matching AuthorizeRouteView's
// own NotAuthorized fallback for the normal (in-circuit) case.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthTokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, TokenAuthStateProvider>();

builder.Services.AddTransient<AuthHeaderHandler>();
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Missing 'ApiBaseUrl' configuration.");
builder.Services.AddHttpClient("VitalisApi", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthHeaderHandler>();
builder.Services.AddScoped<VitalisApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
