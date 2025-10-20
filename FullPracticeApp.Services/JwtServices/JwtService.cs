using FullPracticeApp.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Services.JwtServices
{
    public class JwtService : IJwtService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public JwtService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public int GetUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (!(httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated))
            {
                throw new Exception("User is not authenticated");
            }
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId);
        }
    }
}
