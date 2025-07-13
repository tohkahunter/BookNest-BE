using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly BookTracker7Context _context;

        public BookService(IBookRepository bookRepository, IAuthorRepository authorRepository, BookTracker7Context context)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _context = context;
        }

        // Existing read operations
        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.CreatedByNavigation)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.AuthorId == authorId)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByGenreAsync(int genreId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.GenreId == genreId)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllBooksAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b =>
                    b.Title.ToLower().Contains(searchTerm) ||
                    b.Description.ToLower().Contains(searchTerm) ||
                    b.Isbn13.Contains(searchTerm) ||
                    b.Author.Name.ToLower().Contains(searchTerm))
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        // Enhanced CRUD operations
        public async Task<Book> CreateBookAsync(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (!await IsIsbnUniqueAsync(book.Isbn13))
                throw new InvalidOperationException("A book with this ISBN already exists.");

            // Verify author exists
            var author = await _authorRepository.GetByIdAsync(book.AuthorId);
            if (author == null)
                throw new InvalidOperationException("The specified author does not exist.");

            book.CreatedAt = DateTime.UtcNow;
            await _bookRepository.AddAsync(book);

            // Return book with navigation properties loaded
            return await GetBookByIdAsync(book.BookId);
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            var existingBook = await _bookRepository.GetByIdAsync(book.BookId);
            if (existingBook == null)
                throw new InvalidOperationException("Book not found.");

            if (book.Isbn13 != existingBook.Isbn13 && !await IsIsbnUniqueAsync(book.Isbn13))
                throw new InvalidOperationException("A book with this ISBN already exists.");

            // Verify author exists
            var author = await _authorRepository.GetByIdAsync(book.AuthorId);
            if (author == null)
                throw new InvalidOperationException("The specified author does not exist.");

            // Update properties
            existingBook.Title = book.Title;
            existingBook.Isbn13 = book.Isbn13;
            existingBook.AuthorId = book.AuthorId;
            existingBook.GenreId = book.GenreId;
            existingBook.Description = book.Description;
            existingBook.CoverImageUrl = book.CoverImageUrl;
            existingBook.PublicationYear = book.PublicationYear;
            existingBook.PageCount = book.PageCount;

            _bookRepository.Update(existingBook);

            // Return book with navigation properties loaded
            return await GetBookByIdAsync(existingBook.BookId);
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
                return false;

            // Check if book has user associations or reviews
            var hasUserBooks = await _context.UserBooks.AnyAsync(ub => ub.BookId == id);
            var hasReviews = await _context.Reviews.AnyAsync(r => r.BookId == id);

            if (hasUserBooks || hasReviews)
                throw new InvalidOperationException("Cannot delete book that has user associations or reviews. Consider marking it as inactive instead.");

            _bookRepository.Remove(book);
            return true;
        }

        public async Task<bool> IsIsbnUniqueAsync(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return false;

            var existingBook = await _bookRepository.FirstOrDefaultAsync(b => b.Isbn13 == isbn);
            return existingBook == null;
        }

        // Enhanced methods with statistics
        public async Task<IEnumerable<Book>> GetBooksByUserAsync(int userId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.UserBooks.Any(ub => ub.UserId == userId))
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetPopularBooksAsync(int count)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.UserBooks)
                .OrderByDescending(b => b.UserBooks.Count)
                .ThenBy(b => b.Title)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetRecentlyAddedBooksAsync(int count)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        // Additional admin methods
        public async Task<IEnumerable<Book>> GetBooksWithStatisticsAsync()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.UserBooks)
                .Include(b => b.Reviews)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }
    }
}
