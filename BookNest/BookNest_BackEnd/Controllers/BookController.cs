using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using BookNest_Services.Request;
using BookNest_Services.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookNest_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IReviewService _reviewService;

        public BookController(IBookService bookService, IReviewService reviewService)
        {
            _bookService = bookService;
            _reviewService = reviewService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        #region Public Read Operations

        /// <summary>
        /// Get all books
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            var response = new List<BookResponse>();

            foreach (var book in books)
            {
                var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

                response.Add(new BookResponse
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn13 = book.Isbn13,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author?.Name,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    PublicationYear = book.PublicationYear,
                    PageCount = book.PageCount,
                    CreatedBy = book.CreatedBy,
                    CreatedAt = book.CreatedAt,
                    UserBookCount = book.UserBooks?.Count() ?? 0,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating > 0 ? averageRating : null
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get a book by its ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookResponse>> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound($"Book with ID {id} not found");
            }

            var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
            var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

            var response = new BookResponse
            {
                BookId = book.BookId,
                Title = book.Title,
                Isbn13 = book.Isbn13,
                AuthorId = book.AuthorId,
                AuthorName = book.Author?.Name,
                GenreId = book.GenreId,
                GenreName = book.Genre?.GenreName,
                Description = book.Description,
                CoverImageUrl = book.CoverImageUrl,
                PublicationYear = book.PublicationYear,
                PageCount = book.PageCount,
                CreatedBy = book.CreatedBy,
                CreatedAt = book.CreatedAt,
                UserBookCount = book.UserBooks?.Count() ?? 0,
                ReviewCount = reviewCount,
                AverageRating = averageRating > 0 ? averageRating : null
            };

            return Ok(response);
        }

        /// <summary>
        /// Get books by author ID
        /// </summary>
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooksByAuthor(int authorId)
        {
            var books = await _bookService.GetBooksByAuthorAsync(authorId);
            var response = new List<BookResponse>();

            foreach (var book in books)
            {
                var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

                response.Add(new BookResponse
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn13 = book.Isbn13,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author?.Name,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    PublicationYear = book.PublicationYear,
                    PageCount = book.PageCount,
                    CreatedBy = book.CreatedBy,
                    CreatedAt = book.CreatedAt,
                    UserBookCount = book.UserBooks?.Count() ?? 0,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating > 0 ? averageRating : null
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get books by genre ID
        /// </summary>
        [HttpGet("genre/{genreId}")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetBooksByGenre(int genreId)
        {
            var books = await _bookService.GetBooksByGenreAsync(genreId);
            var response = new List<BookResponse>();

            foreach (var book in books)
            {
                var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

                response.Add(new BookResponse
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn13 = book.Isbn13,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author?.Name,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    PublicationYear = book.PublicationYear,
                    PageCount = book.PageCount,
                    CreatedBy = book.CreatedBy,
                    CreatedAt = book.CreatedAt,
                    UserBookCount = book.UserBooks?.Count() ?? 0,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating > 0 ? averageRating : null
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Search books by title, description, ISBN, or author name
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> SearchBooks([FromQuery] string searchTerm)
        {
            var books = await _bookService.SearchBooksAsync(searchTerm);
            var response = new List<BookResponse>();

            foreach (var book in books)
            {
                var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

                response.Add(new BookResponse
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn13 = book.Isbn13,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author?.Name,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    PublicationYear = book.PublicationYear,
                    PageCount = book.PageCount,
                    CreatedBy = book.CreatedBy,
                    CreatedAt = book.CreatedAt,
                    UserBookCount = book.UserBooks?.Count() ?? 0,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating > 0 ? averageRating : null
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get popular books (most added to user libraries)
        /// </summary>
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetPopularBooks([FromQuery] int count = 10)
        {
            var books = await _bookService.GetPopularBooksAsync(count);
            var response = new List<BookResponse>();

            foreach (var book in books)
            {
                var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

                response.Add(new BookResponse
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn13 = book.Isbn13,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author?.Name,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    PublicationYear = book.PublicationYear,
                    PageCount = book.PageCount,
                    CreatedBy = book.CreatedBy,
                    CreatedAt = book.CreatedAt,
                    UserBookCount = book.UserBooks?.Count() ?? 0,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating > 0 ? averageRating : null
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Get recently added books
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<BookResponse>>> GetRecentBooks([FromQuery] int count = 10)
        {
            var books = await _bookService.GetRecentlyAddedBooksAsync(count);
            var response = new List<BookResponse>();

            foreach (var book in books)
            {
                var averageRating = await _reviewService.GetAverageRatingForBookAsync(book.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(book.BookId);

                response.Add(new BookResponse
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Isbn13 = book.Isbn13,
                    AuthorId = book.AuthorId,
                    AuthorName = book.Author?.Name,
                    GenreId = book.GenreId,
                    GenreName = book.Genre?.GenreName,
                    Description = book.Description,
                    CoverImageUrl = book.CoverImageUrl,
                    PublicationYear = book.PublicationYear,
                    PageCount = book.PageCount,
                    CreatedBy = book.CreatedBy,
                    CreatedAt = book.CreatedAt,
                    UserBookCount = book.UserBooks?.Count() ?? 0,
                    ReviewCount = reviewCount,
                    AverageRating = averageRating > 0 ? averageRating : null
                });
            }

            return Ok(response);
        }

        #endregion

        #region Admin CRUD Operations

        /// <summary>
        /// Create a new book (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateBookResponse>> CreateBook([FromBody] CreateBookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserId();
                var book = new Book
                {
                    Title = request.Title.Trim(),
                    Isbn13 = request.Isbn13.Trim(),
                    AuthorId = request.AuthorId,
                    GenreId = request.GenreId,
                    Description = request.Description?.Trim(),
                    CoverImageUrl = request.CoverImageUrl?.Trim(),
                    PublicationYear = request.PublicationYear,
                    PageCount = request.PageCount,
                    CreatedBy = userId > 0 ? userId : null
                };

                var createdBook = await _bookService.CreateBookAsync(book);

                var response = new CreateBookResponse
                {
                    Message = "Book created successfully",
                    Book = new BookResponse
                    {
                        BookId = createdBook.BookId,
                        Title = createdBook.Title,
                        Isbn13 = createdBook.Isbn13,
                        AuthorId = createdBook.AuthorId,
                        AuthorName = createdBook.Author?.Name,
                        GenreId = createdBook.GenreId,
                        GenreName = createdBook.Genre?.GenreName,
                        Description = createdBook.Description,
                        CoverImageUrl = createdBook.CoverImageUrl,
                        PublicationYear = createdBook.PublicationYear,
                        PageCount = createdBook.PageCount,
                        CreatedBy = createdBook.CreatedBy,
                        CreatedAt = createdBook.CreatedAt,
                        UserBookCount = 0,
                        ReviewCount = 0,
                        AverageRating = null
                    }
                };

                return CreatedAtAction(nameof(GetBookById), new { id = createdBook.BookId }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the book", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing book (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateBookResponse>> UpdateBook(int id, [FromBody] UpdateBookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.BookId)
                return BadRequest("ID in URL does not match ID in request body");

            try
            {
                var book = new Book
                {
                    BookId = request.BookId,
                    Title = request.Title.Trim(),
                    Isbn13 = request.Isbn13.Trim(),
                    AuthorId = request.AuthorId,
                    GenreId = request.GenreId,
                    Description = request.Description?.Trim(),
                    CoverImageUrl = request.CoverImageUrl?.Trim(),
                    PublicationYear = request.PublicationYear,
                    PageCount = request.PageCount
                };

                var updatedBook = await _bookService.UpdateBookAsync(book);

                var averageRating = await _reviewService.GetAverageRatingForBookAsync(updatedBook.BookId);
                var reviewCount = await _reviewService.GetReviewCountForBookAsync(updatedBook.BookId);

                var response = new UpdateBookResponse
                {
                    Message = "Book updated successfully",
                    Book = new BookResponse
                    {
                        BookId = updatedBook.BookId,
                        Title = updatedBook.Title,
                        Isbn13 = updatedBook.Isbn13,
                        AuthorId = updatedBook.AuthorId,
                        AuthorName = updatedBook.Author?.Name,
                        GenreId = updatedBook.GenreId,
                        GenreName = updatedBook.Genre?.GenreName,
                        Description = updatedBook.Description,
                        CoverImageUrl = updatedBook.CoverImageUrl,
                        PublicationYear = updatedBook.PublicationYear,
                        PageCount = updatedBook.PageCount,
                        CreatedBy = updatedBook.CreatedBy,
                        CreatedAt = updatedBook.CreatedAt,
                        UserBookCount = updatedBook.UserBooks?.Count() ?? 0,
                        ReviewCount = reviewCount,
                        AverageRating = averageRating > 0 ? averageRating : null
                    }
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the book", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a book (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DeleteResponse>> DeleteBook(int id)
        {
            try
            {
                var success = await _bookService.DeleteBookAsync(id);
                if (!success)
                {
                    return NotFound(new DeleteResponse
                    {
                        Success = false,
                        Message = "Book not found"
                    });
                }

                return Ok(new DeleteResponse
                {
                    Success = true,
                    Message = "Book deleted successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new DeleteResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new DeleteResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the book"
                });
            }
        }

        /// <summary>
        /// Check if ISBN is unique (Admin helper)
        /// </summary>
        [HttpGet("check-isbn-unique")]
        [Authorize] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> CheckIsbnUnique([FromQuery] string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                return BadRequest("ISBN parameter is required");

            var isUnique = await _bookService.IsIsbnUniqueAsync(isbn.Trim());
            return Ok(isUnique);
        }

        #endregion
    }
}