using Brism;
using FikaWebApp.Components;
using FikaWebApp.Components.Account;
using FikaWebApp.Data;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using MudExtensions.Services;
using Serilog;
using System.Net.Http.Headers;

namespace FikaWebApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File($"{WebAppConfig.LogsPath}/log-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();

        // Add MudBlazor services
        builder.Services.AddMudServices();
        builder.Services.AddMudExtensions();

        // Add Brism
        builder.Services.AddBrism();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add Controllers
        builder.Services.AddControllers();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        var dataDir = WebAppConfig.DataPath;
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        var dbDir = WebAppConfig.DatabasePath;
        if (!Directory.Exists(dbDir))
        {
            Directory.CreateDirectory(dbDir);
        }

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source = {Path.Combine(dbDir, "fikaWebApp.db")}"));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

#if DEBUG
        builder.Services.Configure<WebAppConfig>(builder.Configuration.GetSection("FikaConfig"));

        builder.Services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<WebAppConfig>>().Value);
#else
        var apiKey = Environment.GetEnvironmentVariable("API_KEY")
            ?? throw new Exception("Missing API_KEY");
        var baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
            ?? throw new Exception("Missing BASE_URL");

        var fikaConfig = new FikaConfig()
        {
            APIKey = apiKey,
            BaseUrl = new Uri(baseUrl),
            HeartbeatInterval = 5
        };

        builder.Services.AddSingleton(fikaConfig);
#endif

        builder.Services.AddHttpClient(Options.DefaultName, SetupHttpClient)
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

        builder.Services.AddSingleton<SendTimersService>();
        builder.Services.AddSingleton<ItemCacheService>();
        builder.Services.AddSingleton<HeartbeatService>();
        builder.Services.AddHostedService<BackgroundInitializerService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();
        app.MapControllers();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        using (var scope = app.Services.CreateScope())
        {
            await InitializeDatabase(scope);
        }

        await CheckForSecureFileFolder();
        await CheckForDataFolder();

        await app.RunAsync();
    }

    private static Task CheckForDataFolder()
    {
        var dataPath = WebAppConfig.StoredDataPath;
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        return Task.CompletedTask;
    }

    private static Task CheckForSecureFileFolder()
    {
        var protectedFilesPath = WebAppConfig.ProtectedFilesPath;
        if (!Directory.Exists(protectedFilesPath))
        {
            Directory.CreateDirectory(protectedFilesPath);
        }

        return Task.CompletedTask;
    }

    private static async Task InitializeDatabase(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync(); // ensure DB and tables exist
        await dbContext.Database.EnsureCreatedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roleExists = await roleManager.RoleExistsAsync("Admin");
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        roleExists = await roleManager.RoleExistsAsync("Moderator");
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole("Moderator"));
        }

        var user = await userManager.FindByNameAsync("admin");
        if (user == null)
        {
            user = new()
            {
                UserName = "admin"
            };

            var result = await userManager.CreateAsync(user, "Admin123!");
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create default admin account: \n{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                user = await userManager.FindByNameAsync("admin");
                if (user != null)
                {
                    await userManager.AddToRolesAsync(user, ["Admin", "Moderator"]);
                }
            }
        }
    }

    private static void SetupHttpClient(IServiceProvider provider, HttpClient client)
    {
        var config = provider.GetRequiredService<WebAppConfig>();

        client.BaseAddress = config.BaseUrl;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.APIKey);
        client.DefaultRequestHeaders.Add("requestcompressed", "0");
    }
}
