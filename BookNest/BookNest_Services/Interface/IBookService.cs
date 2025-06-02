using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IBookService
    {
        Task<Book> GetBookByIdAsync(int id);
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
        Task<IEnumerable<Book>> GetBooksByGenreAsync(int genreId);
        Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
        Task<Book> CreateBookAsync(Book book);
        Task<Book> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(int id);
        Task<bool> IsIsbnUniqueAsync(string isbn);
        Task<IEnumerable<Book>> GetBooksByUserAsync(int userId);
        Task<IEnumerable<Book>> GetPopularBooksAsync(int count);
        Task<IEnumerable<Book>> GetRecentlyAddedBooksAsync(int count);
    }
}
