using System.ComponentModel.DataAnnotations;

namespace BookNest_Services.Request.Comment
{
    public class CreateCommentRequest
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Comment must be between 3 and 1000 characters")]
        public string CommentText { get; set; }
    }

    public class UpdateCommentRequest
    {
        [Required]
        public int CommentId { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 3, ErrorMessage = "Comment must be between 3 and 1000 characters")]
        public string CommentText { get; set; }
    }
}