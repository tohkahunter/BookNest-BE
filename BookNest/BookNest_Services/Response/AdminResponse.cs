using System;

namespace BookNest_Services.Response
{
    /// <summary>
    /// Response DTO for Author operations
    /// </summary>
    public class AuthorResponse
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public int BookCount { get; set; }
    }

    /// <summary>
    /// Response DTO for Book operations with full details
    /// </summary>
    public class BookResponse
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Isbn13 { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int? GenreId { get; set; }
        public string GenreName { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public int? PublicationYear { get; set; }
        public int? PageCount { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserBookCount { get; set; } // How many users have this book
        public decimal? AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// Response for create/update operations with success message
    /// </summary>
    public class CreateAuthorResponse
    {
        public string Message { get; set; }
        public AuthorResponse Author { get; set; }
    }

    public class UpdateAuthorResponse
    {
        public string Message { get; set; }
        public AuthorResponse Author { get; set; }
    }

    public class CreateBookResponse
    {
        public string Message { get; set; }
        public BookResponse Book { get; set; }
    }

    public class UpdateBookResponse
    {
        public string Message { get; set; }
        public BookResponse Book { get; set; }
    }

    /// <summary>
    /// Simple message response for delete operations
    /// </summary>
    public class DeleteResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}