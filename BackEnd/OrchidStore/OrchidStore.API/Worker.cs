using OpenIddict.Abstractions;
using OrchidStore.Infrastructure.Data.Helpers;

namespace OrchidStore.API;

/// <summary>
/// Perform the necessary operations when the application starts
/// </summary>
public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="configuration"></param>
    public Worker(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Implement method
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await CreateApplicationsAsync();
        async Task CreateApplicationsAsync()
        {
            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("service_client") == null)
            {
                var clientSecret = _configuration["OpenIddict:ClientSecret"];
                
                var descriptor = new OpenIddictApplicationDescriptor
                {
                    ClientId = "service_client",
                    ClientSecret = clientSecret,
                    DisplayName = "Service client application",
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Introspection,
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.Endpoints.EndSession,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.Password,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.Prefixes.Scope,
                    },
                };
                await manager.CreateAsync(descriptor);
            }
        }
    }
    
    /// <summary>
    ///  Implement method
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

}
