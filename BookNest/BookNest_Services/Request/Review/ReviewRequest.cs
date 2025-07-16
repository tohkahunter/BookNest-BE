using System.ComponentModel.DataAnnotations;

namespace BookNest_Services.Request.Review
{
    public class CreateReviewRequest
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Review text must be between 10 and 2000 characters")]
        public string ReviewText { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        public bool IsPublic { get; set; } = true;
    }

    public class UpdateReviewRequest
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Review text must be between 10 and 2000 characters")]
        public string ReviewText { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}