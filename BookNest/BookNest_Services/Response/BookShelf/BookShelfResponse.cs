using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Response.BookShelf
{
    /// <summary>
    /// Core DTO for a single user‐book entry (in “Want to Read” / “Currently Reading” / “Read”).
    /// </summary>
    public class UserBookResponse
    {
        public int UserBookId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = default!;
        public string AuthorName { get; set; } = default!;
        public string GenreName { get; set; } = default!;
        public string? CoverImageUrl { get; set; }
        public int? PageCount { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = default!;

        public int? ShelfId { get; set; }
        public string? ShelfName { get; set; }

        public DateTime DateAdded { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }

        public int? CurrentPage { get; set; }
        public decimal? ReadingProgress { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Core DTO for a custom or default shelf (“Sci-Fi Classics”, “Favorites”, etc.).
    /// Returned by create/update/get shelf endpoints.
    /// </summary>
    public class BookShelfResponse
    {
        public int ShelfId { get; set; }
        public string ShelfName { get; set; } = default!;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? DisplayOrder { get; set; }
        public int BookCount { get; set; }
    }

    /// <summary>
    /// Core DTO for one of the three reading statuses:
    /// 1 = Want to Read, 2 = Currently Reading, 3 = Read.
    /// </summary>
    public class ReadingStatusResponse
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = default!;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Simple wrapper when the client just needs to know whether a book exists in the user’s library.
    /// </summary>
    public class CheckBookResponse
    {
        public bool Exists { get; set; }
    }

    /// <summary>
    /// Returned by endpoints that only return a “message” and no additional data.
    /// (e.g. remove-book, delete-shelf, move-book without returning the moved entity)
    /// </summary>
    public class MessageResponse
    {
        public string Message { get; set; } = default!;
    }

    /// <summary>
    /// Returned by AddBookToShelf – includes a success message plus the newly created UserBook.
    /// </summary>
    public class AddBookToShelfResponse
    {
        public string Message { get; set; } = default!;
        public UserBookResponse UserBook { get; set; } = default!;
    }

    /// <summary>
    /// Returned by UpdateBookStatus – includes a success message plus the updated UserBook.
    /// </summary>
    public class UpdateBookStatusResponse
    {
        public string Message { get; set; } = default!;
        public UserBookResponse UserBook { get; set; } = default!;
    }

    /// <summary>
    /// Returned by UpdateReadingProgress – includes a success message plus the updated UserBook.
    /// </summary>
    public class UpdateReadingProgressResponse
    {
        public string Message { get; set; } = default!;
        public UserBookResponse UserBook { get; set; } = default!;
    }

    /// <summary>
    /// Returned by CreateCustomShelf – includes a success message plus the created BookShelf.
    /// </summary>
    public class CreateShelfResponse
    {
        public string Message { get; set; } = default!;
        public BookShelfResponse Shelf { get; set; } = default!;
    }

    /// <summary>
    /// Returned by UpdateCustomShelf – includes a success message plus the updated BookShelf.
    /// </summary>
    public class UpdateShelfResponse
    {
        public string Message { get; set; } = default!;
        public BookShelfResponse Shelf { get; set; } = default!;
    }
}
