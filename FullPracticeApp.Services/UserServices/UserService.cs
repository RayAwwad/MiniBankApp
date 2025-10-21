using FullPracticeApp.Contracts.Dtos;
using FullPracticeApp.Contracts.Interfaces;
using FullPracticeApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Services.UserServices
{
    public class UserService : IUserService
    {
        private readonly FullPracticeDbContext dbContext;
        private readonly IJwtService jwt;
        public UserService(FullPracticeDbContext dbContext, IJwtService jwt)
        {
            this.dbContext = dbContext;
            this.jwt = jwt;
        }
        public async Task<UserDetailsDto> UpdateUserInfo(UserDetailsDto dto)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == jwt.GetUserId() && !u.IsDeleted);
            if (user is null)
            {
                throw new Exception("User not found");
            }   
            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            await dbContext.SaveChangesAsync();
            var updatedDto = new UserDetailsDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
            return updatedDto;
        }
        public async Task<List<UserDetailsDto>> GetUsers()
        {
           var info = await dbContext.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserDetailsDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
            }).ToListAsync();
            return info;
        }
        public async Task<UserDetailsDto> GetUserById(int id)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user is null)
            {
                throw new Exception("User not found");
            }
            var info = new UserDetailsDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
            return info;
        }
        public async Task DeleteUser(int id)
        {
           var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
              if (user is null)
              {
                throw new Exception("User not found");
            }
              user.IsDeleted = true;
              user.DeletedAt = DateTime.UtcNow;
              user.DeletedById = jwt.GetUserId();
            await dbContext.SaveChangesAsync();
        }
        public async Task RestoreUser(int id)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted);
            user.IsDeleted = false;
            await dbContext.SaveChangesAsync();
        }
    }
}
