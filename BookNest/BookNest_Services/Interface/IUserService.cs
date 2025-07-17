using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookNest_Services.Request.User;

namespace BookNest_Services.Interface
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserRequest request);
        Task<bool> UpdateUserPasswordAsync(int userId, string newPassword);
        Task<bool> DeleteUserAsync(int userId);
    }
}
