using FullPracticeApp.Contracts.Interfaces;
using FullPracticeApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Services.HttpLogsServices
{
    public class HttpLogsService : IHttpLogsService 
    {
        private readonly FullPracticeDbContext _dbContext;
        public HttpLogsService(FullPracticeDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<object> GetAllLogs()
        {
            var logs = await _dbContext.HttpLogs
             .Select(log => new
                {
                    log.Id,
                    log.Method,
                    log.Path,
                    log.QueryString,
                    log.Body,
                    log.CreatedAt
                }).ToListAsync();

            return logs;
        }
    }
}
