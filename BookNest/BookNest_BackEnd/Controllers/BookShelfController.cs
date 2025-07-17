using BookNest_Services.Interface;
using BookNest_Services.Request.BookShelf;
using BookNest_Services.Response.BookShelf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookNest_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookShelfController : ControllerBase
    {
        private readonly IBookShelfService _bookShelfService;

        public BookShelfController(IBookShelfService bookShelfService)
        {
            _bookShelfService = bookShelfService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        #region Reading Status Management

        [HttpPost("add-book")]
        [Authorize(Roles = "2")]
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

            // Now result.Book, result.Book.Author, result.Book.Genre, result.Status, result.Shelf are all non-null
            var dto = new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            };

            return Ok(new AddBookToShelfResponse
            {
                Message = "Book added (or moved) to your shelf successfully",
                UserBook = dto
            });
        }

        [HttpPut("update-status")]
        [Authorize(Roles = "2")]
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

            var dto = new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            };

            return Ok(new UpdateBookStatusResponse
            {
                Message = "Book status updated successfully",
                UserBook = dto
            });
        }

        [HttpPut("update-progress")]
        [Authorize(Roles = "2")]
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

            var dto = new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            };

            return Ok(new UpdateReadingProgressResponse
            {
                Message = "Reading progress updated successfully",
                UserBook = dto
            });
        }

        [HttpDelete("remove-book/{bookId}")]
        [Authorize(Roles = "2")]
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

        #region View Books by Status/Shelf

        [HttpGet("my-books")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetMyBooks([FromQuery] int? statusId = null, [FromQuery] int? shelfId = null)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, statusId, shelfId);
            var dtoList = userBooks.Select(result => new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet("status/{statusId}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetBooksByStatus(int statusId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, statusId);
            var dtoList = userBooks.Select(result => new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet("shelf/{shelfId}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetBooksByShelf(int shelfId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var userBooks = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, null, shelfId);
            var dtoList = userBooks.Select(result => new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet("book/{bookId}")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetUserBook(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized(new MessageResponse { Message = "Invalid user" });

            var result = await _bookShelfService.GetUserBookAsync(userId, bookId);
            if (result == null)
                return NotFound(new MessageResponse { Message = "Book not found in your library" });

            var dto = new UserBookResponse
            {
                UserBookId = result.UserBookId,
                BookId = result.BookId,
                BookTitle = result.Book.Title,
                AuthorName = result.Book.Author.Name,
                GenreName = result.Book.Genre.GenreName,
                CoverImageUrl = result.Book.CoverImageUrl,
                PageCount = result.Book.PageCount,
                StatusId = result.StatusId,
                StatusName = result.Status.StatusName,
                ShelfId = result.ShelfId,
                ShelfName = result.Shelf?.ShelfName,
                DateAdded = result.DateAdded,
                StartDate = result.StartDate,
                FinishDate = result.FinishDate,
                CurrentPage = result.CurrentPage,
                ReadingProgress = result.ReadingProgress,
                Notes = result.Notes
            };

            return Ok(dto);
        }

        #endregion

        #region Custom Shelves Management

        [HttpPost("create-shelf")]
        [Authorize(Roles = "2")]
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

        [HttpPut("update-shelf")]
        [Authorize(Roles = "2")]

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
                BookCount = result.UserBooks.Count()
            };

            return Ok(new UpdateShelfResponse
            {
                Message = "Shelf updated successfully",
                Shelf = dto
            });
        }

        [HttpDelete("delete-shelf/{shelfId}")]
        [Authorize(Roles = "2")]

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

        [HttpGet("my-shelves")]
        [Authorize(Roles = "2")]

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
                BookCount = s.UserBooks.Count()
            }).ToList();

            return Ok(dtoList);
        }

        [HttpPut("move-book")]
        [Authorize(Roles = "2")]

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

        #region Reading Statuses

        [HttpGet("reading-statuses")]
        [Authorize(Roles = "2")]
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

        [HttpGet("check-book/{bookId}")]
        [Authorize(Roles = "2")]
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
