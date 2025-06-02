using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cce106_palit.Data; // replace with your actual namespace
using Microsoft.EntityFrameworkCore;

public class ExpirationCheckerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpirationCheckerService> _logger;

    public ExpirationCheckerService(IServiceProvider serviceProvider, ILogger<ExpirationCheckerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var today = DateOnly.FromDateTime(DateTime.Today);

                    var products = await context.Products
                        .Where(p => !p.Is_Deleted)
                        .ToListAsync();

                    foreach (var product in products)
                    {
                        var batches = await context.StockIns
                                        .Where(s => s.ProductId == product.Id)
                                      .ToListAsync();

                        if (!batches.Any())
                            continue;

                        bool allBatchesExpired = batches.All(b => b.ExpirationDate <= today);

                        if (allBatchesExpired)
                        {
                            product.Is_Deleted = true;
                            _logger.LogInformation($"Product '{product.Name}' marked as deleted (all batches expired).");
                        }
                        await context.SaveChangesAsync();

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for expired stocks.");
            }

            // Wait for 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
