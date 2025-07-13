using BookNest_Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IAuthorService
    {
        // Existing read operations
        Task<Author> GetAuthorByIdAsync(int id);
        Task<IEnumerable<Author>> GetAllAuthorsAsync();
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);

        // New CRUD operations for admin
        Task<Author> CreateAuthorAsync(Author author);
        Task<Author> UpdateAuthorAsync(Author author);
        Task<bool> DeleteAuthorAsync(int id);
        Task<bool> IsAuthorNameUniqueAsync(string name, int? excludeAuthorId = null);
        Task<bool> CanDeleteAuthorAsync(int authorId); // Check if author has books
        Task<IEnumerable<Author>> SearchAuthorsAsync(string searchTerm);
        Task<IEnumerable<Author>> GetAuthorsWithBookCountAsync();
    }
} 