using FullPracticeApp.Contracts.Dtos;
using FullPracticeApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Contracts.Interfaces
{
    public interface IAuthService
    {
        string GenerateAccessToken(string email, int id);
        string GenerateRefreshToken();
        Task<AuthDto> Signup(SignupDto signupDto);
        Task<AuthDto> Login(LoginDto loginDto);
        Task Logout();
        Task<AuthDto> Refresh(string refreshToken, int userId);
    }
}
