using System.Security.Claims;
using Ansari.Frontend.Services.Handlers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SimpleObjectStore.Admin;
using SimpleObjectStore.Admin.Config;
using SimpleObjectStore.Admin.Services.v1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenIdConfig>(builder.Configuration.GetSection("OpenId"));
builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("API"));

builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#if DEBUG
builder.Services.AddSassCompiler();
#endif

builder.Services.AddScoped<OpenIdConfig>();
builder.Services.AddScoped<ApiConfig>();

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<AccessTokenHandler>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "SimpleObjectStore";
        options.Cookie.Path = "/; SameSite=None";
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
        options.SignedOutRedirectUri = "/signed-out";
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

    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    client.BaseAddress = new Uri(endpoint ?? throw new MissingMemberException("missing endpoint"));
    client.DefaultRequestHeaders.Add("X-API-Key", key);
    client.Timeout = TimeSpan.FromMinutes(30);
});//.AddHttpMessageHandler<AccessTokenHandler>();

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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();