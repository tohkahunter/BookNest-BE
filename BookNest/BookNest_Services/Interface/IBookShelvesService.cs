using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IBookShelfService
    {
        // UserBooks (Reading Status) Management
        Task<UserBook> AddBookToUserShelfAsync(int userId, int bookId, int statusId, int? shelfId = null);
        Task<UserBook> UpdateBookStatusAsync(int userId, int bookId, int newStatusId);
        Task<UserBook> UpdateBookReadingProgressAsync(int userId, int bookId, int? currentPage, decimal? readingProgress, string notes = null);
        Task<bool> RemoveBookFromUserShelfAsync(int userId, int bookId);
        Task<UserBook> GetUserBookAsync(int userId, int bookId);
        Task<IEnumerable<UserBook>> GetUserBooksByStatusAsync(int userId, int statusId);
        Task<IEnumerable<UserBook>> GetAllUserBooksAsync(int userId);
        Task<IEnumerable<UserBook>> GetUserBooksByShelfAsync(int userId, int shelfId);

        // Custom BookShelves Management
        Task<BookShelf> CreateCustomShelfAsync(int userId, string shelfName, string description = null);
        Task<BookShelf> UpdateCustomShelfAsync(int userId, int shelfId, string shelfName, string description = null, int? displayOrder = null);
        Task<bool> DeleteCustomShelfAsync(int userId, int shelfId);
        Task<IEnumerable<BookShelf>> GetUserCustomShelvesAsync(int userId);
        Task<BookShelf> GetShelfByIdAsync(int shelfId);

        // Reading Status Management
        Task<IEnumerable<ReadingStatus>> GetAllReadingStatusesAsync();
        Task<ReadingStatus> GetReadingStatusByIdAsync(int statusId);

        // Combined Views
        Task<IEnumerable<UserBook>> GetUserBooksWithDetailsAsync(int userId, int? statusId = null, int? shelfId = null);
        Task<bool> MoveBookBetweenShelvesAsync(int userId, int bookId, int? newShelfId);
        Task<bool> IsBookInUserLibraryAsync(int userId, int bookId);
    }
}
