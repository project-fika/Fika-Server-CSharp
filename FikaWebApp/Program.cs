using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using FikaWebApp.Data;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

namespace FikaWebApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var silenceLogs = args.Contains("--quiet-logs");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", silenceLogs ? LogEventLevel.Warning : LogEventLevel.Information)
            .WriteTo.Console()
            .WriteTo.File($"{WebAppConfig.LogsPath}/log-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();

        var jwtSecret = builder.Configuration["Jwt:SecretKey"];

        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            var secretKeyPath = Path.Combine(dataDirectory, "jwt-secret.txt");

            Directory.CreateDirectory(dataDirectory);

            if (File.Exists(secretKeyPath))
            {
                jwtSecret = File.ReadAllText(secretKeyPath)
                    .Trim();
            }
            else
            {
                var randomBytes = RandomNumberGenerator.GetBytes(32);
                jwtSecret = Convert.ToBase64String(randomBytes);

                File.WriteAllText(secretKeyPath, jwtSecret);
                Console.WriteLine($"[Security] Generated new persistent JWT secret at: {secretKeyPath}");
            }

            builder.Configuration["Jwt:SecretKey"] = jwtSecret;
        }

        // Add REST Controllers
        builder.Services.AddControllers();

#if DEBUG
        // Configure CORS for local Vite React development
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowViteFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
#endif

        // Configure JWT Secret Key
        var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            throw new InvalidOperationException("Jwt:SecretKey is missing from configuration!");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
        });

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

        // Configure Application Cookie for API-based authentication
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(1);
            options.SlidingExpiration = false;

            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };

            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        // Bind WebAppConfig options section dynamically for both local runs & Docker
        builder.Services.Configure<WebAppConfig>(builder.Configuration.GetSection("FikaConfig"));
        builder.Services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<WebAppConfig>>().Value);

        builder.Services.AddHttpClient(Options.DefaultName, SetupHttpClient)
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                });

        builder.Services.AddSingleton<SendTimersService>();
        builder.Services.AddSingleton<ItemCacheService>();
        builder.Services.AddSingleton<HeartbeatService>();
        builder.Services.AddHostedService<BackgroundInitializerService>();

        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(WebAppConfig.KeysPath))
            .SetApplicationName("FikaWebApp");

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors("AllowViteFrontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapFallbackToFile("index.html");

        using (var scope = app.Services.CreateScope())
        {
            await InitializeDatabase(scope);
        }

        await CheckForSecureFileFolder();
        await CheckForDataFolder();

        if (args.Contains("--reset-admin"))
        {
            await ResetAdminPassword(app);
        }

        await app.RunAsync();
    }

    private static async Task ResetAdminPassword(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                          .CreateLogger("AdminReset");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        logger.LogInformation("Starting admin password reset...");

        var admin = await userManager.FindByNameAsync("admin");

        if (admin == null)
        {
            logger.LogWarning("Admin user not found!");
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(admin);
        var newPassword = "Admin123!";

        var result = await userManager.ResetPasswordAsync(admin, token, newPassword);

        if (!result.Succeeded)
        {
            logger.LogError("Admin password reset failed!");

            foreach (var err in result.Errors)
            {
                logger.LogError("ResetAdminPassword::{Error}", err.Description);
            }

            return;
        }

        logger.LogInformation("Admin password reset successful. New password: {Password}", newPassword);
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

        await dbContext.Database.MigrateAsync();
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