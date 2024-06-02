namespace HotelDemo.Services;

public class CleanupHostedService(IServiceScopeFactory scopeFactory) : IHostedService, IDisposable
{
    private Timer _timer;

    public void Dispose()
    {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10)); // Run cleanup daily
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var cleanupService =
                scope.ServiceProvider.GetRequiredService<UnverifiedUserCleanupService>();
            cleanupService.CleanupUnverifiedUsersAsync().Wait();
        }
    }
}
