using BookNest_Repositories.Models;
using System;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(User user, string password);
        Task<User> AuthenticateUserAsync(string email, string password);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserProfileAsync(int userId, User user);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
        Task<bool> DeleteUserAsync(int userId);
    }
}
