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

namespace FikaWebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
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

            /*builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();*/

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source = fikaWebApp.db"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.Configure<FikaConfig>(builder.Configuration.GetSection("FikaConfig"));
            builder.Services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<FikaConfig>>().Value);

            builder.Services.AddHttpClient(Options.DefaultName, SetupHttpClient)
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });

            builder.Services.AddSingleton<SendTimersService>();
            builder.Services.AddSingleton<ItemCacheService>();
            builder.Services.AddSingleton<HeartbeatService>();

            var app = builder.Build();

            _ = InstantiateSingletons(app);

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

            await CreateSecureFileFolder(app);
            await CheckForDataFolder(app);

            app.Run();
        }

        private static async Task InstantiateSingletons(WebApplication app)
        {
            // this is ugly but it works for now...

            await Task.Delay(TimeSpan.FromSeconds(1)); // artifical delay
            var sendTimerService = app.Services.GetRequiredService<SendTimersService>();
            var itemCacheService = app.Services.GetRequiredService<ItemCacheService>();
            var heartbeatService = app.Services.GetRequiredService<HeartbeatService>();
        }

        private static Task CheckForDataFolder(WebApplication app)
        {
            var dataPath = Path.Combine(app.Environment.ContentRootPath, "StoredData");
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            return Task.CompletedTask;
        }

        private static Task CreateSecureFileFolder(WebApplication app)
        {
            var protectedFilesPath = Path.Combine(app.Environment.ContentRootPath, "ProtectedFiles");
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
            FikaConfig config = provider.GetRequiredService<FikaConfig>();

            client.BaseAddress = config.BaseUrl;
            client.DefaultRequestHeaders.Add("Auth", config.APIKey);
            client.DefaultRequestHeaders.Add("requestcompressed", "0");
        }
    }
}
