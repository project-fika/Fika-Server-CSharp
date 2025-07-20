using FikaWebApp.Components;
using FikaWebApp.Components.Account;
using FikaWebApp.Data;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using System.Net;
using System.Text;

namespace FikaWebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source = fikaWebApp.db"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
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

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await dbContext.Database.MigrateAsync(); // ensure DB and tables exist
                await dbContext.Database.EnsureCreatedAsync();

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
                }
            }

            app.Run();
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
