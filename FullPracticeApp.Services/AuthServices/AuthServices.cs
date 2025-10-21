using FullPracticeApp.Contracts.Dtos;
using FullPracticeApp.Contracts.Interfaces;
using FullPracticeApp.Domain.Entities;
using FullPracticeApp.Infrastructure;
using FullPracticeApp.Infrastructure.Middlewares;
using FullPracticeApp.Services.JwtServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Services.AuthServices
{
    public class AuthServices : IAuthService
    {
        private readonly IJwtService jwt;
        private readonly IConfiguration configuration;
        private readonly FullPracticeDbContext dbContext;
        public AuthServices(IJwtService jwtService, IConfiguration configuration, FullPracticeDbContext dbContext)
        {
            jwt = jwtService;
            this.configuration = configuration;
            this.dbContext = dbContext;
        }
        public string GenerateAccessToken(string email, int id)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var refreshToken = Guid.NewGuid().ToString();
            return refreshToken;
        }
        public async Task<AuthDto> Signup(SignupDto signupDto)
        {
            var exists = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == signupDto.Email.ToLower());
            if(exists != null)
            {
                throw new Exception("User already exists");
            }
            var user = new Users
            {
                FirstName = signupDto.FirstName,
                LastName = signupDto.LastName,
                Email = signupDto.Email,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(signupDto.Password),
                RefreshToken = GenerateRefreshToken(),
                AmountDeposited = 0,
                AmountTransferred = 0,
                AmountWithdrawn = 0,
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            var auth = new AuthDto
            {
                AccessToken = GenerateAccessToken(user.Email, user.Id),
                RefreshToken = user.RefreshToken,
                AccessTokenExpiresAt = DateTime.Now.AddMinutes(15),
                RefreshTokenExpiresAt = DateTime.Now.AddDays(30)
            };
            return auth;
        }
        public async Task<AuthDto> Login(LoginDto loginDto)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(U => U.Email == loginDto.Email);
            if(user is null)
            {
                throw new Exception("User does not exist");
            }
            var isUserValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword);
            if (!isUserValid)
            {
               throw new LoginFailedException("Invalid email or password");
            }
            user.RefreshToken = GenerateRefreshToken();
            await dbContext.SaveChangesAsync();
            var auth = new AuthDto
            {
                AccessToken = GenerateAccessToken(user.Email, user.Id),
                RefreshToken = user.RefreshToken,
                AccessTokenExpiresAt = DateTime.Now.AddMinutes(15),
                RefreshTokenExpiresAt = DateTime.Now.AddDays(30)
            };
            return auth;
        }
        public async Task<AuthDto> Refresh(string refreshToken, int userId)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if(user is null || user.RefreshToken != refreshToken)
            {
                throw new Exception("Invalid refresh token");
            }
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await dbContext.SaveChangesAsync();
            var auth = new AuthDto
            {
                AccessToken = GenerateAccessToken(user.Email, user.Id),
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = DateTime.Now.AddMinutes(15),
                RefreshTokenExpiresAt = DateTime.Now.AddDays(30)
            };
            return auth;
        }
    }
}
