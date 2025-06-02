using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Response.BookShelf
{
    public class UserBookResponse
    {
        public int UserBookId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string AuthorName { get; set; }
        public string GenreName { get; set; }
        public string CoverImageUrl { get; set; }
        public int? PageCount { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public int? ShelfId { get; set; }
        public string ShelfName { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public int? CurrentPage { get; set; }
        public decimal? ReadingProgress { get; set; }
        public string Notes { get; set; }
    }

    public class BookShelfResponse
    {
        public int ShelfId { get; set; }
        public string ShelfName { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? DisplayOrder { get; set; }
        public int BookCount { get; set; }
    }

    public class ReadingStatusResponse
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
    }
}
