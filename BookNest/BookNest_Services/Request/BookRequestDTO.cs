using System.ComponentModel.DataAnnotations;

namespace BookNest_Services.Request
{
    public class CreateBookRequest
    {
        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
        public string Isbn13 { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public int? GenreId { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Url(ErrorMessage = "Please provide a valid URL for the cover image")]
        public string CoverImageUrl { get; set; }

        [Range(1000, 2100, ErrorMessage = "Publication year must be between 1000 and 2100")]
        public int? PublicationYear { get; set; }

        [Range(1, 10000, ErrorMessage = "Page count must be between 1 and 10000")]
        public int? PageCount { get; set; }
    }

    public class UpdateBookRequest
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN must be between 10 and 13 characters")]
        public string Isbn13 { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public int? GenreId { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Url(ErrorMessage = "Please provide a valid URL for the cover image")]
        public string CoverImageUrl { get; set; }

        [Range(1000, 2100, ErrorMessage = "Publication year must be between 1000 and 2100")]
        public int? PublicationYear { get; set; }

        [Range(1, 10000, ErrorMessage = "Page count must be between 1 and 10000")]
        public int? PageCount { get; set; }
    }
}