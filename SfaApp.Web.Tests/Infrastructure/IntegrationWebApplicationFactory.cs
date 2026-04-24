using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SfaApp.Web.Data;
using SfaApp.Web.Services;

namespace SfaApp.Web.Tests.Infrastructure;

public sealed class IntegrationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public IntegrationWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var mySqlProviderDescriptors = services
                .Where(d =>
                    d.ServiceType.Assembly.GetName().Name?.Contains("Pomelo.EntityFrameworkCore.MySql", StringComparison.Ordinal) == true ||
                    d.ImplementationType?.Assembly.GetName().Name?.Contains("Pomelo.EntityFrameworkCore.MySql", StringComparison.Ordinal) == true ||
                    d.ImplementationInstance?.GetType().Assembly.GetName().Name?.Contains("Pomelo.EntityFrameworkCore.MySql", StringComparison.Ordinal) == true)
                .ToList();

            foreach (var descriptor in mySqlProviderDescriptors)
            {
                services.Remove(descriptor);
            }

            var dbContextConfigurationDescriptors = services
                .Where(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))
                .ToList();

            foreach (var descriptor in dbContextConfigurationDescriptors)
            {
                services.Remove(descriptor);
            }

            var dbContextOptions = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                .ToList();

            foreach (var descriptor in dbContextOptions)
            {
                services.Remove(descriptor);
            }

            var dbContextRegistration = services.FirstOrDefault(d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextRegistration is not null)
            {
                services.Remove(dbContextRegistration);
            }

            var hostedServicesToRemove = services
                .Where(d => d.ServiceType == typeof(IHostedService)
                    && (d.ImplementationType == typeof(UploadJobBackgroundService)
                        || d.ImplementationType == typeof(SyncQueueBackgroundService)))
                .ToList();

            foreach (var descriptor in hostedServicesToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.Configure<SyncTransportOptions>(options =>
            {
                options.Enabled = false;
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
