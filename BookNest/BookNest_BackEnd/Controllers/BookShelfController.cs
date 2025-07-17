using BookNest_Services.Interface;
using BookNest_Services.Request.BookShelf;
using BookNest_Services.Response.BookShelf;
using BookNest_Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookNest_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookShelfController : ControllerBase
    {
        private readonly IBookShelfService _bookShelfService;
        private readonly BookTracker7Context _context;

        public BookShelfController(IBookShelfService bookShelfService, BookTracker7Context context)
        {
            _bookShelfService = bookShelfService;
            _context = context;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task<UserBookResponse> CreateUserBookResponse(UserBook result)
        {
            // Ensure navigation properties are loaded
            if (result.Book == null)
            {
                await _context.Entry(result).Reference(r => r.Book).LoadAsync();
            }

            if (result.Book != null)
            {
                if (result.Book.Author == null)
                {
                    await _context.Entry(result.Book).Reference(b => b.Author).LoadAsync();
                }
                if (result.Book.Genre == null)
                {
                    await _context.Entry(result.Book).Reference(b => b.Genre).LoadAsync();
                }
            }

            if (result.Status == null)
            {
                await _context.Entry(result).Reference(r => r.Status).LoadAsync();
            }

            if (result.ShelfId.HasValue && result.Shelf == null)
            {
                await _context.Entry(result).Reference(r => r.Shelf).LoadAsync();
            }

            return new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book?.Title ?? "Unknown Title",
                AuthorName = result.Book?.Author?.Name ?? "Unknown Author",
                GenreName = result.Book?.Genre?.GenreName ?? "Unknown Genre",
                CoverImageUrl = result.Book?.CoverImageUrl,
                PageCount = result.Book?.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status?.StatusName ?? "Unknown Status",
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            };
        }

        #region Reading Status Management

        /// <summary>
        /// Add a book to your personal library with specific reading status and optional shelf
        /// Usage: POST /api/BookShelf/add-book
        /// Body: { "bookId": 1, "statusId": 3, "shelfId": 5 }
        /// StatusIds: 1=Want to Read, 2=Currently Reading, 3=Read
        /// </summary>
        [HttpPost("add-book")]
        public async Task<IActionResult> AddBookToShelf([FromBody] AddBookToShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService
                .AddOrUpdateShelfForExistingBookAsync(userId, request.BookId, request.StatusId, request.ShelfId);

            if (result == null)
                return BadRequest(new MessageResponse { Message = "Could not add or update that book." });

            var dto = await CreateUserBookResponse(result);

            return Ok(new AddBookToShelfResponse
            {
                Message = "Book added (or moved) to your shelf successfully",
                UserBook = dto
            });
        }

        /// <summary>
        /// Update a book's reading status (e.g., from "Want to Read" to "Currently Reading")
        /// Usage: PUT /api/BookShelf/update-status
        /// Body: { "bookId": 1, "newStatusId": 2 }
        /// Auto-updates: Sets start/finish dates based on status
        /// </summary>
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateBookStatus([FromBody] UpdateBookStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService.UpdateBookStatusAsync(userId, request.BookId, request.NewStatusId);
            if (result == null)
                return NotFound(new MessageResponse { Message = "Book not found in your library" });

            var dto = await CreateUserBookResponse(result);

            return Ok(new UpdateBookStatusResponse
            {
                Message = "Book status updated successfully",
                UserBook = dto
            });
        }

        /// <summary>
        /// Update your reading progress with page numbers, percentage, and personal notes
        /// Usage: PUT /api/BookShelf/update-progress
        /// Body: { "bookId": 1, "currentPage": 150, "readingProgress": 75.5, "notes": "Great chapter!" }
        /// Auto-complete: Sets status to "Read" when progress reaches 100%
        /// </summary>
        [HttpPut("update-progress")]
        public async Task<IActionResult> UpdateReadingProgress([FromBody] UpdateReadingProgressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService.UpdateBookReadingProgressAsync(
                userId, request.BookId, request.CurrentPage, request.ReadingProgress, request.Notes);

            if (result == null)
                return NotFound(new MessageResponse { Message = "Book not found in your library" });

            var dto = await CreateUserBookResponse(result);

            return Ok(new UpdateReadingProgressResponse
            {
                Message = "Reading progress updated successfully",
                UserBook = dto
            });
        }

        /// <summary>
        /// Remove a book completely from your personal library
        /// Usage: DELETE /api/BookShelf/remove-book/1
        /// Warning: This deletes all your reading progress and notes for this book
        /// </summary>
        [HttpDelete("remove-book/{bookId}")]
        public async Task<IActionResult> RemoveBookFromShelf(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var success = await _bookShelfService.RemoveBookFromUserShelfAsync(userId, bookId);
            if (!success)
                return NotFound(new MessageResponse { Message = "Book not found in your library" });

            return Ok(new MessageResponse { Message = "Book removed from your library successfully" });
        }

        #endregion

        #region View Your Books

        /// <summary>
        /// Get all books in your library with optional filtering
        /// Usage: GET /api/BookShelf/my-books
        /// Filters: ?statusId=2 (Currently Reading), ?shelfId=5 (Specific shelf), ?statusId=2&shelfId=5 (Both)
        /// Response: List of all your books with reading progress
        /// </summary>
        [HttpGet("my-books")]
        public async Task<IActionResult> GetMyBooks([FromQuery] int? statusId = null, [FromQuery] int? shelfId = null)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, statusId, shelfId);
            var dtoList = new List<UserBookResponse>();

            foreach (var result in userBooks)
            {
                var dto = await CreateUserBookResponse(result);
                dtoList.Add(dto);
            }

            return Ok(dtoList);
        }

        /// <summary>
        /// Get all books with a specific reading status
        /// Usage: GET /api/BookShelf/status/1 (Want to Read), /status/2 (Currently Reading), /status/3 (Read)
        /// Use Cases: Show "Currently Reading" dashboard, "Want to Read" wishlist, completed books
        /// </summary>
        [HttpGet("status/{statusId}")]
        public async Task<IActionResult> GetBooksByStatus(int statusId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, statusId);
            var dtoList = new List<UserBookResponse>();

            foreach (var result in userBooks)
            {
                var dto = await CreateUserBookResponse(result);
                dtoList.Add(dto);
            }

            return Ok(dtoList);
        }

        /// <summary>
        /// Get all books organized in a specific custom shelf
        /// Usage: GET /api/BookShelf/shelf/5
        /// Use Cases: Show "Favorites" shelf, "2024 Reading Challenge" shelf, etc.
        /// </summary>
        [HttpGet("shelf/{shelfId}")]
        public async Task<IActionResult> GetBooksByShelf(int shelfId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, null, shelfId);
            var dtoList = new List<UserBookResponse>();

            foreach (var result in userBooks)
            {
                var dto = await CreateUserBookResponse(result);
                dtoList.Add(dto);
            }

            return Ok(dtoList);
        }

        /// <summary>
        /// Get detailed reading information for a specific book in your library
        /// Usage: GET /api/BookShelf/book/1
        /// Response: Full book details with your personal reading progress, notes, and shelf info
        /// </summary>
        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetUserBook(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService.GetUserBookAsync(userId, bookId);
            if (result == null)
                return NotFound(new MessageResponse { Message = "Book not found in your library" });

            var dto = await CreateUserBookResponse(result);

            return Ok(dto);
        }

        /// <summary>
        /// Get ALL books in your personal library (Current user's complete library view)
        /// Usage: GET /api/BookShelf/all-my-books
        /// Purpose: View your complete library with all reading statuses and progress
        /// Use Cases: Personal dashboard, library overview, reading statistics
        /// </summary>
        [HttpGet("all-my-books")]
        public async Task<IActionResult> GetAllMyBooks()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId);
            var dtoList = new List<UserBookResponse>();

            foreach (var result in userBooks)
            {
                var dto = await CreateUserBookResponse(result);
                dtoList.Add(dto);
            }

            return Ok(new
            {
                UserId = userId,
                TotalBooks = dtoList.Count,
                WantToRead = dtoList.Count(b => b.StatusId == 1),
                CurrentlyReading = dtoList.Count(b => b.StatusId == 2),
                Read = dtoList.Count(b => b.StatusId == 3),
                Books = dtoList
            });
        }

        ///// <summary>
        ///// Get ALL books in a specific user's library (Admin/Public view)
        ///// Usage: GET /api/BookShelf/user/5/books
        ///// Purpose: View another user's complete library with all reading statuses and progress
        ///// Use Cases: Public profile view, admin monitoring, social features
        ///// </summary>
        //[HttpGet("user/{userId}/books")]
        //public async Task<IActionResult> GetUserBooks(int userId)
        //{
        //    var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId);
        //    var dtoList = new List<UserBookResponse>();

        //    foreach (var result in userBooks)
        //    {
        //        var dto = await CreateUserBookResponse(result);
        //        dtoList.Add(dto);
        //    }

        //    return Ok(new
        //    {
        //        UserId = userId,
        //        TotalBooks = dtoList.Count,
        //        WantToRead = dtoList.Count(b => b.StatusId == 1),
        //        CurrentlyReading = dtoList.Count(b => b.StatusId == 2),
        //        Read = dtoList.Count(b => b.StatusId == 3),
        //        Books = dtoList
        //    });
        //}

        #endregion

        #region Custom Shelves Management

        /// <summary>
        /// Create a personal organizational shelf for your books
        /// Usage: POST /api/BookShelf/create-shelf
        /// Body: { "shelfName": "2024 Reading Challenge", "description": "Books I want to read this year" }
        /// Examples: "Favorites", "Book Club Picks", "Summer Reading", "Work Related"
        /// </summary>
        [HttpPost("create-shelf")]
        public async Task<IActionResult> CreateCustomShelf([FromBody] CreateShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService.CreateCustomShelfAsync(userId, request.ShelfName, request.Description);
            if (result == null)
                return BadRequest(new MessageResponse { Message = "Shelf name already exists" });

            var dto = new BookShelfResponse
            {
                ShelfId = result.ShelfId,
                ShelfName = result.ShelfName,
                Description = result.Description,
                IsDefault = result.IsDefault,
                CreatedAt = result.CreatedAt,
                DisplayOrder = result.DisplayOrder,
                BookCount = 0
            };

            return Ok(new CreateShelfResponse
            {
                Message = "Custom shelf created successfully",
                Shelf = dto
            });
        }

        /// <summary>
        /// Modify existing shelf name, description, or display order
        /// Usage: PUT /api/BookShelf/update-shelf
        /// Body: { "shelfId": 5, "shelfName": "Updated Name", "description": "Updated description", "displayOrder": 1 }
        /// Note: Cannot update default system shelves
        /// </summary>
        [HttpPut("update-shelf")]
        public async Task<IActionResult> UpdateCustomShelf([FromBody] UpdateShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService.UpdateCustomShelfAsync(
                userId, request.ShelfId, request.ShelfName, request.Description, request.DisplayOrder);

            if (result == null)
                return BadRequest(new MessageResponse { Message = "Shelf not found or name already exists" });

            var dto = new BookShelfResponse
            {
                ShelfId = result.ShelfId,
                ShelfName = result.ShelfName,
                Description = result.Description,
                IsDefault = result.IsDefault,
                CreatedAt = result.CreatedAt,
                DisplayOrder = result.DisplayOrder,
                BookCount = result.UserBooks?.Count() ?? 0
            };

            return Ok(new UpdateShelfResponse
            {
                Message = "Shelf updated successfully",
                Shelf = dto
            });
        }

        /// <summary>
        /// Remove a custom shelf (books remain in library, just lose shelf assignment)
        /// Usage: DELETE /api/BookShelf/delete-shelf/5
        /// Protection: Cannot delete default system shelves
        /// </summary>
        [HttpDelete("delete-shelf/{shelfId}")]
        public async Task<IActionResult> DeleteCustomShelf(int shelfId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var success = await _bookShelfService.DeleteCustomShelfAsync(userId, shelfId);
            if (!success)
                return BadRequest(new MessageResponse { Message = "Shelf not found or cannot be deleted" });

            return Ok(new MessageResponse { Message = "Shelf deleted successfully" });
        }

        /// <summary>
        /// List all your custom shelves with book counts
        /// Usage: GET /api/BookShelf/my-shelves
        /// Response: Shelf details including how many books are in each shelf
        /// </summary>
        [HttpGet("my-shelves")]
        public async Task<IActionResult> GetMyCustomShelves()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var shelves = await _bookShelfService.GetUserCustomShelvesAsync(userId);
            var dtoList = shelves.Select(s => new BookShelfResponse
            {
                ShelfId = s.ShelfId,
                ShelfName = s.ShelfName,
                Description = s.Description,
                IsDefault = s.IsDefault,
                CreatedAt = s.CreatedAt,
                DisplayOrder = s.DisplayOrder,
                BookCount = s.UserBooks?.Count() ?? 0
            }).ToList();

            return Ok(dtoList);
        }

        /// <summary>
        /// Move a book from one shelf to another (or remove from shelf by setting to null)
        /// Usage: PUT /api/BookShelf/move-book
        /// Body: { "bookId": 1, "newShelfId": 7 }
        /// Note: Book stays in your library, just changes shelf organization
        /// </summary>
        [HttpPut("move-book")]
        public async Task<IActionResult> MoveBookToShelf([FromBody] MoveBookToShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var success = await _bookShelfService.MoveBookBetweenShelvesAsync(userId, request.BookId, request.NewShelfId);
            if (!success)
                return BadRequest(new MessageResponse { Message = "Book or shelf not found" });

            return Ok(new MessageResponse { Message = "Book moved successfully" });
        }

        #endregion

        #region Reading Statuses (Reference)

        /// <summary>
        /// Get list of all reading statuses (Want to Read, Currently Reading, Read)
        /// Usage: GET /api/BookShelf/reading-statuses
        /// Public: No authentication required
        /// Use Case: Populate status dropdowns in your frontend
        /// </summary>
        [HttpGet("reading-statuses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReadingStatuses()
        {
            var statuses = await _bookShelfService.GetAllReadingStatusesAsync();
            var dtoList = statuses.Select(s => new ReadingStatusResponse
            {
                StatusId = s.StatusId,
                StatusName = s.StatusName,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder
            }).ToList();

            return Ok(dtoList);
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// Quick check if a specific book exists in your library
        /// Usage: GET /api/BookShelf/check-book/1
        /// Response: { "exists": true/false }
        /// Use Case: Show "Add to Library" vs "Already in Library" buttons
        /// </summary>
        [HttpGet("check-book/{bookId}")]
        public async Task<IActionResult> CheckBookInLibrary(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var exists = await _bookShelfService.IsBookInUserLibraryAsync(userId, bookId);
            return Ok(new CheckBookResponse { Exists = exists });
        }

        #endregion
    }
}