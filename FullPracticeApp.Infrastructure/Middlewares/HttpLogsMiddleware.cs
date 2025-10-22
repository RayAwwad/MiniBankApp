using FullPracticeApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Infrastructure.Middlewares
{
    public class HttpLogsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public HttpLogsMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            request.EnableBuffering();

            string requestBody = string.Empty;

            if (request.ContentLength != null && request.ContentLength > 0)
            {
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();

                request.Body.Position = 0;
            }

            var log = new
            {
                request.Method,
                request.Path,
                QueryString = request.QueryString.ToString(),
                Body = requestBody,
                CreatedAt = DateTime.UtcNow
            };

            // created a new scope to get the DbContext since middlewares are singleton by default
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<FullPracticeDbContext>();

            dbContext.HttpLogs.Add(new HttpLog
            {
                Method = log.Method,
                Path = log.Path,
                QueryString = log.QueryString,
                Body = log.Body,
                CreatedAt = log.CreatedAt
            });

            await dbContext.SaveChangesAsync();

            await _next(context);
        }
    }
}
