using FullPracticeApp.Contracts.Dtos;
using FullPracticeApp.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FullPracticeApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminEmail")]
    public class UserController : Controller
    {
        private readonly IUserService userService;
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }


        [HttpPut("update-info")]
        public async Task<IActionResult> UpdateUserDetails(UserDetailsDto dto)
        {
            var updatedDto = await userService.UpdateUserInfo(dto);
            return Ok(updatedDto);
        }

        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await userService.GetUsers();
            return Ok(users);
        }
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await userService.GetUserById(id);
            return Ok(user);
        }

        [HttpDelete("delete-user")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await userService.DeleteUser(id);
            return Ok("User deleted successfully");
        }
        [HttpPut("restore-user")]
        public async Task<IActionResult> RestoreUser(int id)
        {
            await userService.RestoreUser(id);
            return Ok("User restored successfully");
        }
    }
}
