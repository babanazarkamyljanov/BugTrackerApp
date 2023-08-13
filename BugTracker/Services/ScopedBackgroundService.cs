using BugTracker.Data.SeedData;

namespace BugTracker.Services;

public class ScopedBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScopedBackgroundService> _logger;

    public ScopedBackgroundService(IServiceProvider serviceProvider, ILogger<ScopedBackgroundService> logger) =>
        (_serviceProvider, _logger) = (serviceProvider, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(ScopedBackgroundService)} is running");
        await DoWorkAsync(stoppingToken);
    }

    private async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            $"{nameof(ScopedBackgroundService)} is working.");

        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            try
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await SeedDefaultRoles.SeedAsync(roleManager);
            }
            catch (Exception ex)
            {

                _logger.LogWarning(ex, $"An error occured while seeding DB. Error message: {ex.Message}");
            }
        }
    }
}
