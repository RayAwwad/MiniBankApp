using FullPracticeApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Services
{
    public class LogsCleanupService : BackgroundService
    {
        //could've used IHostedService and IDisposable instead of BackgroundService but BackgroundService will handle the thread management and i can only override ExecuteAsync

        private readonly IServiceProvider _serviceProvider;
        public LogsCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
             await CleanupOldLogsAsync();

             await Task.Delay(TimeSpan.FromDays(2), stoppingToken);
            }
        }

        private async Task CleanupOldLogsAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<FullPracticeDbContext>();

            var twoDaysBefore = DateTime.UtcNow.AddDays(-2);

            var oldLogs = await dbContext.HttpLogs
                .Where(log => log.CreatedAt < twoDaysBefore)
                .ToListAsync();

            if (oldLogs.Count > 0)
            {
                dbContext.HttpLogs.RemoveRange(oldLogs);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}