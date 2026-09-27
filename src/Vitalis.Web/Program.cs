using Microsoft.AspNetCore.Components.Authorization;
using Vitalis.Web.Components;
using Vitalis.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth: no ASP.NET Core cookie/JWT middleware here — Vitalis.Web only needs
// component-level AuthorizeView/[Authorize] checks (AddAuthorizationCore),
// backed by AuthTokenStore/TokenAuthStateProvider instead of a real auth handler.
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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
