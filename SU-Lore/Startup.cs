using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using SU_Lore.Data.RichText;
using SU_Lore.Database;
using SU_Lore.Helpers;
using SU_Lore.Pages.Shared;

namespace SU_Lore;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddControllersWithViews().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (Configuration.GetConnectionString("DefaultConnection") == null)
            {
                Log.Fatal("No connection string found in appsettings.json. Exiting.");
                Environment.Exit(1);
            }

            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        });

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            var proxyIP = Configuration["ProxyIP"];
            if (proxyIP == null)
            {
                Log.Fatal("No proxy IP found in appsettings.json. Exiting.");
                Environment.Exit(1);
            }

            Log.Information("Proxy IP: {ProxyIP}", proxyIP);
            options.KnownProxies.Add(IPAddress.Parse(proxyIP));
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = int.MaxValue;
        });

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = int.MaxValue;
        });

        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = int.MaxValue;
            });

        services.AddHttpContextAccessor();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        }).AddCookie("Cookies", options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
        }).AddOpenIdConnect("oidc", options =>
        {
            options.SignInScheme = "Cookies";

            options.Authority = "https://central.spacestation14.io/web/";
            options.ClientId = Configuration["ClientId"];
            options.ClientSecret = Configuration["ClientSecret"];

            options.ResponseType = OpenIdConnectResponseType.Code;
            options.RequireHttpsMetadata = false;

            options.Scope.Add("openid");
            options.Scope.Add("profile");

            options.GetClaimsFromUserInfoEndpoint = true;
        });

        services.AddSwaggerGen();
        services.AddHttpLogging(o => { });

        services.AddScoped<AuthenticationHelper>();
        services.AddScoped<PageReader>();
        services.AddScoped<RichTextParser>();
        services.AddScoped<RandomQuoteHelper>();

        services.AddSingleton<AnimationHelper>();

        // Run migrations on startup.
        var sw = Stopwatch.StartNew();
        Log.Information("Applying migrations...");
        try
        {
            var dbContext = services.BuildServiceProvider().GetService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            Log.Information("Migrations applied in {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to apply migrations.");
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHttpLogging();

        app.Use((context, next) =>
        {
            // Log the request.
            Log.Information("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
            context.Request.Scheme = "https";
            return next();
        });

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapSwagger();
            endpoints.MapControllers();
            endpoints.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "api/{controller=Home}/{action=Index}/{id?}");
        });

        app.UseHttpsRedirection();
        app.UseForwardedHeaders();
        app.UseRouting();
        app.UseCors();

        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        } else
        {
            app.UseDeveloperExceptionPage();
        }
    }
}