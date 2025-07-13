using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;
        private readonly BookTracker7Context _context;

        public AuthorService(IAuthorRepository authorRepository, IBookRepository bookRepository, BookTracker7Context context)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
            _context = context;
        }

        // Existing read operations
        public async Task<Author> GetAuthorByIdAsync(int id)
        {
            return await _authorRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            return await _authorRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
        {
            return await _bookRepository.FindAsync(b => b.AuthorId == authorId);
        }

        // New CRUD operations for admin
        public async Task<Author> CreateAuthorAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            if (!await IsAuthorNameUniqueAsync(author.Name))
                throw new InvalidOperationException("An author with this name already exists.");

            await _authorRepository.AddAsync(author);
            return author;
        }

        public async Task<Author> UpdateAuthorAsync(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            var existingAuthor = await GetAuthorByIdAsync(author.AuthorId);
            if (existingAuthor == null)
                throw new InvalidOperationException("Author not found.");

            if (!await IsAuthorNameUniqueAsync(author.Name, author.AuthorId))
                throw new InvalidOperationException("An author with this name already exists.");

            existingAuthor.Name = author.Name;
            _authorRepository.Update(existingAuthor);
            return existingAuthor;
        }

        public async Task<bool> DeleteAuthorAsync(int id)
        {
            var author = await GetAuthorByIdAsync(id);
            if (author == null)
                return false;

            // Check if author has books - prevent deletion if they do
            if (!await CanDeleteAuthorAsync(id))
                throw new InvalidOperationException("Cannot delete author who has published books. Please reassign or delete the books first.");

            _authorRepository.Remove(author);
            return true;
        }

        public async Task<bool> IsAuthorNameUniqueAsync(string name, int? excludeAuthorId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var existingAuthor = await _authorRepository.FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());

            if (existingAuthor == null)
                return true;

            // If we're updating an existing author, exclude it from the uniqueness check
            return excludeAuthorId.HasValue && existingAuthor.AuthorId == excludeAuthorId.Value;
        }

        public async Task<bool> CanDeleteAuthorAsync(int authorId)
        {
            var books = await _bookRepository.FindAsync(b => b.AuthorId == authorId);
            return !books.Any();
        }

        public async Task<IEnumerable<Author>> SearchAuthorsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAuthorsAsync();

            searchTerm = searchTerm.ToLower();
            return await _authorRepository.FindAsync(a => a.Name.ToLower().Contains(searchTerm));
        }

        public async Task<IEnumerable<Author>> GetAuthorsWithBookCountAsync()
        {
            return await _context.Authors
                .Include(a => a.Books)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
    }
} 