using BookNest_Services.Interface;
using BookNest_Services.Request.User;
using BookNest_Services.Service;
using BookNest_Repositories.Models;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]UserRegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userService.RegisterUserAsync(user, request.Password);
            if (result == null)
                return BadRequest("Email or username already exists");

            return Ok(new { message = "Account created successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.AuthenticateUserAsync(user.Email, user.Password);
            if (result == null)
                return Unauthorized("Invalid email or password");

            var token = _jwtService.GenerateToken(result.UserId.ToString(), result.Email, result.RoleId);
            return Ok(new { 
                user = new { 
                    id = result.UserId,
                    email = result.Email,
                    username = result.Username,
                    roleId = result.RoleId,
                }, 
                token = token 
            });
        }
    }
}