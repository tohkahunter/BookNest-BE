using BookNest_Services.Interface;
using BookNest_Services.Request.BookShelf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookNest_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
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
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        #region Reading Status Management

        [HttpPost("add-book")]
        public async Task<IActionResult> AddBookToShelf([FromBody] AddBookToShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.AddBookToUserShelfAsync(userId, request.BookId, request.StatusId, request.ShelfId);
            if (result == null)
                return BadRequest("Book already exists in your library");

            return Ok(new { message = "Book added to your library successfully", userBook = result });
        }

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateBookStatus([FromBody] UpdateBookStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.UpdateBookStatusAsync(userId, request.BookId, request.NewStatusId);
            if (result == null)
                return NotFound("Book not found in your library");

            return Ok(new { message = "Book status updated successfully", userBook = result });
        }

        [HttpPut("update-progress")]
        public async Task<IActionResult> UpdateReadingProgress([FromBody] UpdateReadingProgressRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.UpdateBookReadingProgressAsync(
                userId, request.BookId, request.CurrentPage, request.ReadingProgress, request.Notes);

            if (result == null)
                return NotFound("Book not found in your library");

            return Ok(new { message = "Reading progress updated successfully", userBook = result });
        }

        [HttpDelete("remove-book/{bookId}")]
        public async Task<IActionResult> RemoveBookFromShelf(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.RemoveBookFromUserShelfAsync(userId, bookId);
            if (!result)
                return NotFound("Book not found in your library");

            return Ok(new { message = "Book removed from your library successfully" });
        }

        #endregion

        #region View Books by Status/Shelf

        [HttpGet("my-books")]
        public async Task<IActionResult> GetMyBooks([FromQuery] int? statusId = null, [FromQuery] int? shelfId = null)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var books = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, statusId, shelfId);
            return Ok(books);
        }

        [HttpGet("status/{statusId}")]
        public async Task<IActionResult> GetBooksByStatus(int statusId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var books = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, statusId);
            return Ok(books);
        }

        [HttpGet("shelf/{shelfId}")]
        public async Task<IActionResult> GetBooksByShelf(int shelfId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var books = await _bookShelfService.GetUserBooksWithDetailsAsync(userId, null, shelfId);
            return Ok(books);
        }

        [HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetUserBook(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var userBook = await _bookShelfService.GetUserBookAsync(userId, bookId);
            if (userBook == null)
                return NotFound("Book not found in your library");

            return Ok(userBook);
        }

        #endregion

        #region Custom Shelves Management

        [HttpPost("create-shelf")]
        public async Task<IActionResult> CreateCustomShelf([FromBody] CreateShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.CreateCustomShelfAsync(userId, request.ShelfName, request.Description);
            if (result == null)
                return BadRequest("Shelf name already exists");

            return Ok(new { message = "Custom shelf created successfully", shelf = result });
        }

        [HttpPut("update-shelf")]
        public async Task<IActionResult> UpdateCustomShelf([FromBody] UpdateShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.UpdateCustomShelfAsync(
                userId, request.ShelfId, request.ShelfName, request.Description, request.DisplayOrder);

            if (result == null)
                return BadRequest("Shelf not found or name already exists");

            return Ok(new { message = "Shelf updated successfully", shelf = result });
        }

        [HttpDelete("delete-shelf/{shelfId}")]
        public async Task<IActionResult> DeleteCustomShelf(int shelfId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.DeleteCustomShelfAsync(userId, shelfId);
            if (!result)
                return BadRequest("Shelf not found or cannot be deleted");

            return Ok(new { message = "Shelf deleted successfully" });
        }

        [HttpGet("my-shelves")]
        public async Task<IActionResult> GetMyCustomShelves()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var shelves = await _bookShelfService.GetUserCustomShelvesAsync(userId);
            return Ok(shelves);
        }

        [HttpPut("move-book")]
        public async Task<IActionResult> MoveBookToShelf([FromBody] MoveBookToShelfRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var result = await _bookShelfService.MoveBookBetweenShelvesAsync(userId, request.BookId, request.NewShelfId);
            if (!result)
                return BadRequest("Book or shelf not found");

            return Ok(new { message = "Book moved successfully" });
        }

        #endregion

        #region Reading Statuses

        [HttpGet("reading-statuses")]
        [AllowAnonymous] // Allow visitors to see available statuses
        public async Task<IActionResult> GetReadingStatuses()
        {
            var statuses = await _bookShelfService.GetAllReadingStatusesAsync();
            return Ok(statuses);
        }

        #endregion

        #region Utility Endpoints

        [HttpGet("check-book/{bookId}")]
        public async Task<IActionResult> CheckBookInLibrary(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var exists = await _bookShelfService.IsBookInUserLibraryAsync(userId, bookId);
            return Ok(new { exists = exists });
        }

        #endregion
    }
}
