using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class BookShelfService : IBookShelfService
    {
        private readonly IUserBooksRepository _userBooksRepository;
        private readonly IBookShelvesRepository _bookShelvesRepository;
        private readonly IReadingStatusRepository _readingStatusRepository;
        private readonly BookTracker7Context _context;

        public BookShelfService(
            IUserBooksRepository userBooksRepository,
            IBookShelvesRepository bookShelvesRepository,
            IReadingStatusRepository readingStatusRepository,
            BookTracker7Context context)
        {
            _userBooksRepository = userBooksRepository;
            _bookShelvesRepository = bookShelvesRepository;
            _readingStatusRepository = readingStatusRepository;
            _context = context;
        }

        // UserBooks (Reading Status) Management
        public async Task<UserBook> AddBookToUserShelfAsync(int userId, int bookId, int statusId, int? shelfId = null)
        {
            // Check if book already exists in user's library
            var existingUserBook = await _userBooksRepository.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (existingUserBook != null)
                return null; // Book already exists

            var userBook = new UserBook
            {
                UserId = userId,
                BookId = bookId,
                StatusId = statusId,
                ShelfId = shelfId,
                DateAdded = DateTime.UtcNow,
                StartDate = statusId == 2 ? DateTime.UtcNow : null, // If "Currently Reading"
                ReadingProgress = 0.00m
            };

            await _userBooksRepository.AddAsync(userBook);
            return userBook;
        }

        public async Task<UserBook> UpdateBookStatusAsync(int userId, int bookId, int newStatusId)
        {
            var userBook = await _userBooksRepository.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (userBook == null)
                return null;

            userBook.StatusId = newStatusId;

            // Update dates based on status
            switch (newStatusId)
            {
                case 2: // Currently Reading
                    if (userBook.StartDate == null)
                        userBook.StartDate = DateTime.UtcNow;
                    userBook.FinishDate = null;
                    break;
                case 3: // Read
                    if (userBook.StartDate == null)
                        userBook.StartDate = DateTime.UtcNow;
                    userBook.FinishDate = DateTime.UtcNow;
                    userBook.ReadingProgress = 100.00m;
                    break;
                case 1: // Want to Read
                    // Keep existing dates if any
                    break;
            }

            _userBooksRepository.Update(userBook);
            return userBook;
        }

        public async Task<UserBook> UpdateBookReadingProgressAsync(int userId, int bookId, int? currentPage, decimal? readingProgress, string notes = null)
        {
            var userBook = await _userBooksRepository.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (userBook == null)
                return null;

            if (currentPage.HasValue)
                userBook.CurrentPage = currentPage.Value;

            if (readingProgress.HasValue)
                userBook.ReadingProgress = readingProgress.Value;

            if (!string.IsNullOrEmpty(notes))
                userBook.Notes = notes;

            // If progress is 100%, automatically mark as "Read"
            if (readingProgress.HasValue && readingProgress.Value >= 100.00m && userBook.StatusId != 3)
            {
                userBook.StatusId = 3; // Read status
                userBook.FinishDate = DateTime.UtcNow;
            }

            _userBooksRepository.Update(userBook);
            return userBook;
        }

        public async Task<bool> RemoveBookFromUserShelfAsync(int userId, int bookId)
        {
            var userBook = await _userBooksRepository.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (userBook == null)
                return false;

            _userBooksRepository.Remove(userBook);
            return true;
        }

        public async Task<UserBook> GetUserBookAsync(int userId, int bookId)
        {
            return await _userBooksRepository.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        }

        public async Task<IEnumerable<UserBook>> GetUserBooksByStatusAsync(int userId, int statusId)
        {
            return await _userBooksRepository.FindAsync(ub => ub.UserId == userId && ub.StatusId == statusId);
        }

        public async Task<IEnumerable<UserBook>> GetAllUserBooksAsync(int userId)
        {
            return await _userBooksRepository.FindAsync(ub => ub.UserId == userId);
        }

        public async Task<IEnumerable<UserBook>> GetUserBooksByShelfAsync(int userId, int shelfId)
        {
            return await _userBooksRepository.FindAsync(ub => ub.UserId == userId && ub.ShelfId == shelfId);
        }

        // Custom BookShelves Management
        public async Task<BookShelf> CreateCustomShelfAsync(int userId, string shelfName, string description = null)
        {
            // Check if shelf name already exists for user
            var existingShelf = await _bookShelvesRepository.FirstOrDefaultAsync(bs => bs.UserId == userId && bs.ShelfName == shelfName);
            if (existingShelf != null)
                return null;

            var shelf = new BookShelf
            {
                UserId = userId,
                ShelfName = shelfName,
                Description = description,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            await _bookShelvesRepository.AddAsync(shelf);
            return shelf;
        }

        public async Task<BookShelf> UpdateCustomShelfAsync(int userId, int shelfId, string shelfName, string description = null, int? displayOrder = null)
        {
            var shelf = await _bookShelvesRepository.FirstOrDefaultAsync(bs => bs.ShelfId == shelfId && bs.UserId == userId);
            if (shelf == null || shelf.IsDefault) // Can't update default shelves
                return null;

            // Check if new name conflicts with existing shelves
            var existingShelf = await _bookShelvesRepository.FirstOrDefaultAsync(bs => bs.UserId == userId && bs.ShelfName == shelfName && bs.ShelfId != shelfId);
            if (existingShelf != null)
                return null;

            shelf.ShelfName = shelfName;
            shelf.Description = description;
            if (displayOrder.HasValue)
                shelf.DisplayOrder = displayOrder.Value;

            _bookShelvesRepository.Update(shelf);
            return shelf;
        }

        public async Task<bool> DeleteCustomShelfAsync(int userId, int shelfId)
        {
            var shelf = await _bookShelvesRepository.FirstOrDefaultAsync(bs => bs.ShelfId == shelfId && bs.UserId == userId);
            if (shelf == null || shelf.IsDefault) // Can't delete default shelves
                return false;

            // Remove shelf assignment from all books
            var booksInShelf = await _userBooksRepository.FindAsync(ub => ub.ShelfId == shelfId);
            foreach (var userBook in booksInShelf)
            {
                userBook.ShelfId = null;
                _userBooksRepository.Update(userBook);
            }

            _bookShelvesRepository.Remove(shelf);
            return true;
        }

        public async Task<IEnumerable<BookShelf>> GetUserCustomShelvesAsync(int userId)
        {
            return await _bookShelvesRepository.FindAsync(bs => bs.UserId == userId);
        }

        public async Task<BookShelf> GetShelfByIdAsync(int shelfId)
        {
            return await _bookShelvesRepository.GetByIdAsync(shelfId);
        }

        // Reading Status Management
        public async Task<IEnumerable<ReadingStatus>> GetAllReadingStatusesAsync()
        {
            return await _readingStatusRepository.GetAllAsync();
        }

        public async Task<ReadingStatus> GetReadingStatusByIdAsync(int statusId)
        {
            return await _readingStatusRepository.GetByIdAsync(statusId);
        }

        // Combined Views
        public async Task<IEnumerable<UserBook>> GetUserBooksWithDetailsAsync(int userId, int? statusId = null, int? shelfId = null)
        {
            var query = _context.UserBooks
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Author)
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Genre)
                .Include(ub => ub.Status)
                .Include(ub => ub.Shelf)
                .Where(ub => ub.UserId == userId);

            if (statusId.HasValue)
                query = query.Where(ub => ub.StatusId == statusId.Value);

            if (shelfId.HasValue)
                query = query.Where(ub => ub.ShelfId == shelfId.Value);

            return await query.OrderBy(ub => ub.DateAdded).ToListAsync();
        }

        public async Task<bool> MoveBookBetweenShelvesAsync(int userId, int bookId, int? newShelfId)
        {
            var userBook = await _userBooksRepository.FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == bookId);
            if (userBook == null)
                return false;

            // If moving to a specific shelf, verify it belongs to the user
            if (newShelfId.HasValue)
            {
                var shelf = await _bookShelvesRepository.FirstOrDefaultAsync(bs => bs.ShelfId == newShelfId.Value && bs.UserId == userId);
                if (shelf == null)
                    return false;
            }

            userBook.ShelfId = newShelfId;
            _userBooksRepository.Update(userBook);
            return true;
        }

        public async Task<bool> IsBookInUserLibraryAsync(int userId, int bookId)
        {
            return await _userBooksRepository.ExistsAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        }
    }
}
