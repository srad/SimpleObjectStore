using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using SimpleObjectStore.Admin.Services;
using SimpleObjectStore.Services.v1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = new PathString("/access-denied");
        options.Cookie.Name = "SimpleObjectStore";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddOpenIdConnect(options =>
    {
        var authority = builder.Configuration["OpenId:Authority"];
        var clientId = builder.Configuration["OpenId:ClientId"];
        var clientSecret = builder.Configuration["OpenId:ClientSecret"];

        if (authority == null) throw new MissingFieldException("OpenId authority missing");
        if (clientId == null) throw new MissingFieldException("OpenId client-id missing");
        if (clientSecret == null) throw new MissingFieldException("OpenId client-secret missing");

        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.AccessDeniedPath = new PathString("/access-denied");
        options.Authority = authority;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.UseTokenLifetime = false;
        options.RequireHttpsMetadata = builder.Environment.IsProduction() && !(builder.Configuration["DisableHttpsMetadata"] != null && builder.Configuration["DisableHttpsMetadata"] == "true");
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name", RoleClaimType = ClaimTypes.Role };

        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.Response.Redirect("/access-denied");
                ctx.HandleResponse();
                return Task.CompletedTask;
            },

            OnAccessDenied = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/access-denied");
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddHttpClient("api", client =>
{
    var endpoint = builder.Configuration["API:Endpoint"];
    var key = builder.Configuration["API:Key"];

    client.BaseAddress = new Uri(endpoint ?? throw new MissingMemberException("missing endpoint"));
    client.DefaultRequestHeaders.Add("X-API-Key", key);
    client.Timeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddScoped<SimpleObjectStoreClient>(x =>
{
    var factory = x.GetRequiredService<IHttpClientFactory>();
    var endpoint = builder.Configuration["API:Endpoint"] ?? throw new MissingMemberException("missing endpoint");
    var httpClient = factory.CreateClient("api");

    return new SimpleObjectStoreClient(endpoint, httpClient);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized || response.StatusCode == (int)HttpStatusCode.Forbidden)
        response.Redirect("/login");
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();