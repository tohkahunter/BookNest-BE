using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Get all books
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        /// <summary>
        /// Get a book by its ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }
            return Ok(book);
        }

        /// <summary>
        /// Get books by author ID
        /// </summary>
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByAuthor(int authorId)
        {
            var books = await _bookService.GetBooksByAuthorAsync(authorId);
            return Ok(books);
        }
    }
} 