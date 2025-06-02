using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            return await _bookRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _bookRepository.FindAsync(b => b.AuthorId == authorId);
        }

        public async Task<IEnumerable<Book>> GetBooksByGenreAsync(int genreId)
        {
            return await _bookRepository.FindAsync(b => b.GenreId == genreId);
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllBooksAsync();

            searchTerm = searchTerm.ToLower();
            return await _bookRepository.FindAsync(b =>
                b.Title.ToLower().Contains(searchTerm) ||
                b.Description.ToLower().Contains(searchTerm) ||
                b.Isbn13.Contains(searchTerm));
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (!await IsIsbnUniqueAsync(book.Isbn13))
                throw new InvalidOperationException("A book with this ISBN already exists.");

            book.CreatedAt = DateTime.UtcNow;
            await _bookRepository.AddAsync(book);
            return book;
        }

        public async Task<Book> UpdateBookAsync(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            var existingBook = await GetBookByIdAsync(book.BookId);
            if (existingBook == null)
                throw new InvalidOperationException("Book not found.");

            if (book.Isbn13 != existingBook.Isbn13 && !await IsIsbnUniqueAsync(book.Isbn13))
                throw new InvalidOperationException("A book with this ISBN already exists.");

            _bookRepository.Update(book);
            return book;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await GetBookByIdAsync(id);
            if (book == null)
                return false;

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

        public async Task<IEnumerable<Book>> GetBooksByUserAsync(int userId)
        {
            var books = await _bookRepository.IncludeAsync(b => b.UserBooks);
            return books.Where(b => b.UserBooks.Any(ub => ub.UserId == userId));
        }

        public async Task<IEnumerable<Book>> GetPopularBooksAsync(int count)
        {
            var books = await _bookRepository.IncludeAsync(b => b.UserBooks);
            return books
                .OrderByDescending(b => b.UserBooks.Count)
                .Take(count);
        }

        public async Task<IEnumerable<Book>> GetRecentlyAddedBooksAsync(int count)
        {
            var books = await _bookRepository.GetAllAsync();
            return books
                .OrderByDescending(b => b.CreatedAt)
                .Take(count);
        }
    }
}
