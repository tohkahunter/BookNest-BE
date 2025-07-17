using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using BookNest_Services.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class AuthenService : IAuthenService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;

        public AuthenService(IUserRepository userRepository, IUserService userService)
        {
            _userRepository = userRepository;
            _userService = userService;
        }

        public async Task<User> RegisterUserAsync(User user, string password)
        {
            if (await IsEmailUniqueAsync(user.Email) && await IsUsernameUniqueAsync(user.Username))
            {
                user.Password = PasswordHasher.HashPassword(password);
                user.RegistrationDate = DateTime.UtcNow;
                user.IsActive = true;
                user.RoleId = 2; // Default role for regular users

                await _userRepository.AddAsync(user);
                return user;
            }
            return null;
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user != null && PasswordHasher.VerifyPassword(password, user.Password))
            {
                user.LastLoginDate = DateTime.UtcNow;
                _userRepository.Update(user);
                return user;
            }
            return null;
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            var users = await _userRepository.FindAsync(u => u.Email == email);
            return !users.Any();
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            var users = await _userRepository.FindAsync(u => u.Username == username);
            return !users.Any();
        }
    }
}
