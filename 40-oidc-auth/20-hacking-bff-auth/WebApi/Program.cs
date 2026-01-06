using System.Text.RegularExpressions;
using AppServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using WebApi;

var builder = WebApplication.CreateBuilder(args);

// These settings make sure that everything works during development (localhost without certs)
// and during production (with certs). It also solves all CORS issues.
// TODO: Speak with someone who knows more about security. Is it ok? Probably yes 😅

builder.Services.AddDbContext<ApplicationDataContext>(options =>
{
    var path = builder.Configuration["Database:path"] ?? throw new InvalidOperationException("Database path not configured.");
    var fileName = builder.Configuration["Database:fileName"] ?? throw new InvalidOperationException("Database file name not configured.");
    options.UseSqlite($"Data Source={path}/{fileName}");
});
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = false;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "VulnerableAuthCookie";

    options.ExpireTimeSpan = TimeSpan.FromHours(24*365);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(options =>
{
    options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["MicrosoftEntraID:TenantId"]}/v2.0";
    options.ClientId = builder.Configuration["MicrosoftEntraID:ClientId"];
    options.ClientSecret = builder.Configuration["MicrosoftEntraID:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true
    };
    
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", (HttpContext context, [FromQuery] string? redirect) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        if (!string.IsNullOrEmpty(redirect))
        {
            return Results.Redirect(redirect, preserveMethod: true);
        }

        return Results.Ok(new { message = "Logged in" });
    }

    return Results.Challenge(authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]);
}).WithDescription("Redirects to the login page.").Produces(StatusCodes.Status200OK).Produces(StatusCodes.Status307TemporaryRedirect);

app.MapGet("/logout", (HttpContext context, [FromQuery] string? redirect) =>
{
    // Delete chunked cookies (ASP.NET Core creates these when cookie is too large)
    context.Response.Cookies.Delete("VulnerableAuthCookie");
    for (int i = 1; i <= 10; i++)
    {
        context.Response.Cookies.Delete($"VulnerableAuthCookieC{i}");
    }

    if (!string.IsNullOrEmpty(redirect))
    {
        return Results.Redirect(redirect, preserveMethod: true);
    }

    return Results.Ok(new { message = "Logged out" });
}).WithDescription("Logs out the user and redirects to the specified URL.").Produces(StatusCodes.Status200OK).Produces(StatusCodes.Status307TemporaryRedirect);

app.MapGet("/me", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
    return Results.Ok(new {
        authenticated = true,
        name = context.User.Identity.Name,
        claims
    });
}).WithDescription("Returns the user's claims.").Produces(StatusCodes.Status200OK).Produces(StatusCodes.Status401Unauthorized);

var group = app.MapGroup("");
group.MapCustomerEndpoints();
group.MapSecretsEndpoints();
group.RequireAuthorization();

app.Run();
