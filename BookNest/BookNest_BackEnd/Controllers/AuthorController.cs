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
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        #region Public Read Operations

        /// <summary>
        /// Get all authors
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAllAuthors()
        {
            var authors = await _authorService.GetAuthorsWithBookCountAsync();
            var response = authors.Select(a => new AuthorResponse
            {
                AuthorId = a.AuthorId,
                Name = a.Name,
                BookCount = a.Books?.Count() ?? 0
            });
            return Ok(response);
        }

        /// <summary>
        /// Get an author by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorResponse>> GetAuthorById(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found");
            }

            var books = await _authorService.GetBooksByAuthorAsync(id);
            var response = new AuthorResponse
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                BookCount = books.Count()
            };

            return Ok(response);
        }

        /// <summary>
        /// Get all books by an author
        /// </summary>
        [HttpGet("{id}/books")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByAuthor(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
            {
                return NotFound($"Author with ID {id} not found");
            }

            var books = await _authorService.GetBooksByAuthorAsync(id);
            return Ok(books);
        }

        /// <summary>
        /// Search authors by name
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> SearchAuthors([FromQuery] string searchTerm)
        {
            var authors = await _authorService.SearchAuthorsAsync(searchTerm);
            var response = authors.Select(a => new AuthorResponse
            {
                AuthorId = a.AuthorId,
                Name = a.Name,
                BookCount = a.Books?.Count() ?? 0
            });
            return Ok(response);
        }

        #endregion

        #region Admin CRUD Operations

        /// <summary>
        /// Create a new author (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "3")] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateAuthorResponse>> CreateAuthor([FromBody] CreateAuthorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var author = new Author
                {
                    Name = request.Name.Trim()
                };

                var createdAuthor = await _authorService.CreateAuthorAsync(author);

                var response = new CreateAuthorResponse
                {
                    Message = "Author created successfully",
                    Author = new AuthorResponse
                    {
                        AuthorId = createdAuthor.AuthorId,
                        Name = createdAuthor.Name,
                        BookCount = 0
                    }
                };

                return CreatedAtAction(nameof(GetAuthorById), new { id = createdAuthor.AuthorId }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the author", details = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing author (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "3")] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateAuthorResponse>> UpdateAuthor(int id, [FromBody] UpdateAuthorRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.AuthorId)
                return BadRequest("ID in URL does not match ID in request body");

            try
            {
                var author = new Author
                {
                    AuthorId = request.AuthorId,
                    Name = request.Name.Trim()
                };

                var updatedAuthor = await _authorService.UpdateAuthorAsync(author);
                var books = await _authorService.GetBooksByAuthorAsync(updatedAuthor.AuthorId);

                var response = new UpdateAuthorResponse
                {
                    Message = "Author updated successfully",
                    Author = new AuthorResponse
                    {
                        AuthorId = updatedAuthor.AuthorId,
                        Name = updatedAuthor.Name,
                        BookCount = books.Count()
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
                return StatusCode(500, new { message = "An error occurred while updating the author", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete an author (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "3")] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DeleteResponse>> DeleteAuthor(int id)
        {
            try
            {
                var success = await _authorService.DeleteAuthorAsync(id);
                if (!success)
                {
                    return NotFound(new DeleteResponse
                    {
                        Success = false,
                        Message = "Author not found"
                    });
                }

                return Ok(new DeleteResponse
                {
                    Success = true,
                    Message = "Author deleted successfully"
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
                    Message = "An error occurred while deleting the author"
                });
            }
        }

        /// <summary>
        /// Check if author name is unique (Admin helper)
        /// </summary>
        [HttpGet("check-name-unique")]
        [Authorize(Roles = "3")] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> CheckAuthorNameUnique([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name parameter is required");

            var isUnique = await _authorService.IsAuthorNameUniqueAsync(name.Trim(), excludeId);
            return Ok(isUnique);
        }

        /// <summary>
        /// Check if author can be deleted (Admin helper)
        /// </summary>
        //[HttpGet("{id}/can-delete")]
        //[Authorize] // Add role-based authorization as needed: [Authorize(Roles = "Admin")]
        //public async Task<ActionResult<bool>> CanDeleteAuthor(int id)
        //{
        //    var canDelete = await _authorService.CanDeleteAuthorAsync(id);
        //    return Ok(canDelete);
        //}

        #endregion
    }
}