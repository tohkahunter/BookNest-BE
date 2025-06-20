﻿using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using BookNest_Services.Utilities;
using System;
using System.Linq;
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
            var user = await GetUserByEmailAsync(email);
            if (user != null && PasswordHasher.VerifyPassword(password, user.Password))
            {
                user.LastLoginDate = DateTime.UtcNow;
                _userRepository.Update(user);
                return user;
            }
            return null;
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

        public async Task<bool> UpdateUserProfileAsync(int userId, User user)
        {
            var existingUser = await GetUserByIdAsync(userId);
            if (existingUser == null) return false;

            // Update only allowed fields
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.ProfilePictureUrl = user.ProfilePictureUrl;

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

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            _userRepository.Remove(user);
            return true;
        }
    }
}
