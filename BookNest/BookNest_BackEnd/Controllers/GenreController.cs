using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BookNest_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly BookTracker7Context _context;

        public GenreController(BookTracker7Context context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all genres
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAllGenres()
        {
            var genres = await _context.Genres.ToListAsync();
            var response = genres.Select(g => new GenreResponse
            {
                GenreId = g.GenreId,
                GenreName = g.GenreName,
                Description = g.Description,
                BookCount = g.Books?.Count() ?? 0
            });
            return Ok(response);
        }

        /// <summary>
        /// Get a genre by ID
        /// </summary>
        [HttpGet("{id}")]

        public async Task<ActionResult<GenreResponse>> GetGenreById(int id)
        {
            var genre = await _context.Genres
                .Include(g => g.Books)
                .FirstOrDefaultAsync(g => g.GenreId == id);

            if (genre == null)
                return NotFound($"Genre with ID {id} not found");

            var response = new GenreResponse
            {
                GenreId = genre.GenreId,
                GenreName = genre.GenreName,
                Description = genre.Description,
                BookCount = genre.Books?.Count() ?? 0
            };

            return Ok(response);
        }

        /// <summary>
        /// Create a new genre (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles ="3")] // Add role-based authorization as needed
        public async Task<ActionResult<CreateGenreResponse>> CreateGenre([FromBody] CreateGenreRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if genre name already exists
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.GenreName.ToLower() == request.GenreName.ToLower());

            if (existingGenre != null)
                return BadRequest("A genre with this name already exists");

            var genre = new Genre
            {
                GenreName = request.GenreName.Trim(),
                Description = request.Description?.Trim()
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            var response = new CreateGenreResponse
            {
                Message = "Genre created successfully",
                Genre = new GenreResponse
                {
                    GenreId = genre.GenreId,
                    GenreName = genre.GenreName,
                    Description = genre.Description,
                    BookCount = 0
                }
            };

            return CreatedAtAction(nameof(GetGenreById), new { id = genre.GenreId }, response);
        }

        /// <summary>
        /// Update an existing genre (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "3")] // Add role-based authorization as needed
        public async Task<ActionResult<UpdateGenreResponse>> UpdateGenre(int id, [FromBody] UpdateGenreRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.GenreId)
                return BadRequest("ID in URL does not match ID in request body");

            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound("Genre not found");

            // Check if new name conflicts with existing genres (excluding current)
            var existingGenre = await _context.Genres
                .FirstOrDefaultAsync(g => g.GenreName.ToLower() == request.GenreName.ToLower() && g.GenreId != id);

            if (existingGenre != null)
                return BadRequest("A genre with this name already exists");

            // Update the genre properties
            genre.GenreName = request.GenreName.Trim();
            genre.Description = request.Description?.Trim();

            // Mark the entity as modified and save changes
            _context.Genres.Update(genre);
            await _context.SaveChangesAsync();

            var bookCount = await _context.Books.CountAsync(b => b.GenreId == id);

            var response = new UpdateGenreResponse
            {
                Message = "Genre updated successfully",
                Genre = new GenreResponse
                {
                    GenreId = genre.GenreId,
                    GenreName = genre.GenreName,
                    Description = genre.Description,
                    BookCount = bookCount
                }
            };

            return Ok(response);
        }

        /// <summary>
        /// Delete a genre (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "3")] // Add role-based authorization as needed
        public async Task<ActionResult<DeleteResponse>> DeleteGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null)
                return NotFound(new DeleteResponse { Success = false, Message = "Genre not found" });

            // Check if genre has books
            var hasBooks = await _context.Books.AnyAsync(b => b.GenreId == id);
            if (hasBooks)
            {
                return BadRequest(new DeleteResponse
                {
                    Success = false,
                    Message = "Cannot delete genre that has books associated with it. Please reassign the books first."
                });
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Genre deleted successfully"
            });
        }
    }

    // Genre DTOs
    public class GenreResponse
    {
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public string Description { get; set; }
        public int BookCount { get; set; }
    }

    public class CreateGenreRequest
    {
        [Required]
        [StringLength(50, ErrorMessage = "Genre name cannot exceed 50 characters")]
        public string GenreName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }
    }

    public class UpdateGenreRequest
    {
        [Required]
        public int GenreId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Genre name cannot exceed 50 characters")]
        public string GenreName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }
    }

    public class CreateGenreResponse
    {
        public string Message { get; set; }
        public GenreResponse Genre { get; set; }
    }

    public class UpdateGenreResponse
    {
        public string Message { get; set; }
        public GenreResponse Genre { get; set; }
    }
}