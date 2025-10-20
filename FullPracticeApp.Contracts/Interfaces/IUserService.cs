using FullPracticeApp.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullPracticeApp.Contracts.Interfaces
{
    public interface IUserService
    {
        Task<UserDetailsDto> UpdateUserInfo(UserDetailsDto dto);
        Task<List<UserDetailsDto>> GetUsers();
        Task<UserDetailsDto> GetUserById(int id);
        Task DeleteUser(int id);
        Task RestoreUser(int id);
    }
}
