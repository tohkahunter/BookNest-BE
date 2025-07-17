using BookNest_Repositories.Models;
using System;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IAuthenService
    {
        Task<User> RegisterUserAsync(User user, string password);
        Task<User> AuthenticateUserAsync(string email, string password);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<bool> IsUsernameUniqueAsync(string username);
    }
}
