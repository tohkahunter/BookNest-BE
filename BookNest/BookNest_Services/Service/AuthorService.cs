using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookService _bookService;

        public AuthorService(IAuthorRepository authorRepository, IBookService bookService)
        {
            _authorRepository = authorRepository;
            _bookService = bookService;
        }

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
            return await _bookService.GetBooksByAuthorAsync(authorId);
        }
    }
} 