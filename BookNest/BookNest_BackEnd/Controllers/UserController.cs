using Microsoft.AspNetCore.Mvc;
using BookNest_Services.Interface;
using BookNest_Repositories.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace BookNest_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [Authorize(Roles = "3")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(Roles = "3")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] User user)
        {
            var result = await _userService.UpdateUserProfileAsync(id, user);
            if (!result) return NotFound();
            return Ok(new { message = "User profile updated successfully" });
        }

        [Authorize(Roles = "3")]
        [HttpPut("{id}/password")]
        public async Task<IActionResult> UpdateUserPassword(int id, [FromBody] string newPassword)
        {
            var result = await _userService.UpdateUserPasswordAsync(id, newPassword);
            if (!result) return NotFound();
            return Ok(new { message = "User password updated successfully" });
        }

        [Authorize(Roles = "2")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted successfully" });
        }
    }
}
