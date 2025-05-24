using BookNest_Services.Interface;
using BookNest_Services.Request.User;
using BookNest_Services.Service;
using Microsoft.AspNetCore.Mvc;

namespace BookNest_BackEnd.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly JwtService _jwtService;

        public UserController(IUserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.AuthenticateUserAsync(user.Email, user.Password);
            if (result == null)
                return Unauthorized("Invalid email or password");

            var token = _jwtService.GenerateToken(result.UserId.ToString(), result.Email);
            return Ok(new { 
                user = new { 
                    id = result.UserId,
                    email = result.Email,
                    username = result.Username
                }, 
                token = token 
            });
        }
    }
}