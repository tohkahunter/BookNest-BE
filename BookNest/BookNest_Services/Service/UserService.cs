using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using BookNest_Services.Utilities;
using BookNest_Services.Request.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var users = await _userRepository.FindAsync(u => u.Email == email);
            return users.FirstOrDefault();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var users = await _userRepository.FindAsync(u => u.Username == username);
            return users.FirstOrDefault();
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserRequest request)
        {
            var existingUser = await GetUserByIdAsync(userId);
            if (existingUser == null) return false;

            if (!string.IsNullOrEmpty(request.Username))
                existingUser.Username = request.Username;
            if (!string.IsNullOrEmpty(request.Email))
                existingUser.Email = request.Email;
            if (!string.IsNullOrEmpty(request.FirstName))
                existingUser.FirstName = request.FirstName;
            if (!string.IsNullOrEmpty(request.LastName))
                existingUser.LastName = request.LastName;
            if (!string.IsNullOrEmpty(request.ProfilePictureUrl))
                existingUser.ProfilePictureUrl = request.ProfilePictureUrl;
            if (request.IsActive.HasValue)
                existingUser.IsActive = request.IsActive.Value;

            _userRepository.Update(existingUser);
            return true;
        }

        public async Task<bool> UpdateUserPasswordAsync(int userId, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            user.Password = PasswordHasher.HashPassword(newPassword);
            _userRepository.Update(user);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            _userRepository.Remove(user);
            return true;
        }
    }
}
