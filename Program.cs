using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;

var builder = WebApplication.CreateBuilder(args);

//  Configure MSAL authentication with Microsoft Graph API
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(["User.Read", "Calendars.Read"])
    .AddInMemoryTokenCaches();

builder.Services.AddRazorPages();

//  Register Microsoft Graph's Authentication Provider
// builder.Services.AddScoped<IAuthenticationProvider, TokenAcquisitionAuthenticationProvider>();

//  Register GraphServiceClient with the correct `IRequestAdapter`
builder.Services.AddScoped<IRequestAdapter>(provider =>
{
    var authProvider = provider.GetRequiredService<IAuthenticationProvider>();
    return new HttpClientRequestAdapter(authProvider);
});

builder.Services.AddScoped<GraphServiceClient>(provider =>
{
    var requestAdapter = provider.GetRequiredService<IRequestAdapter>();
    return new GraphServiceClient(requestAdapter);
});

//  Register GraphService
builder.Services.AddScoped<GraphService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
