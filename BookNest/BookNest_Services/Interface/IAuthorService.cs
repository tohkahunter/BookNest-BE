using BookNest_Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IAuthorService
    {
        Task<Author> GetAuthorByIdAsync(int id);
        Task<IEnumerable<Author>> GetAllAuthorsAsync();
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
    }
} 