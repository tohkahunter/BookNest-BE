using System;
using System.Collections.Generic;

namespace BookNest_Services.Response.Review
{
    public class ReviewResponse
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string UserFullName { get; set; }
        public string UserProfilePictureUrl { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public DateTime DateReviewed { get; set; }
        public bool IsPublic { get; set; }
        public int CommentCount { get; set; }
        public bool CanEdit { get; set; } // True if current user owns this review
        public bool CanDelete { get; set; } // True if current user owns this review or is admin
    }

    public class CommentResponse
    {
        public int CommentId { get; set; }
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string UserFullName { get; set; }
        public string UserProfilePictureUrl { get; set; }
        public string CommentText { get; set; }
        public DateTime DateCommented { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool CanEdit { get; set; } // True if current user owns this comment
        public bool CanDelete { get; set; } // True if current user owns this comment or is admin
    }

    public class BookReviewSummaryResponse
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public List<ReviewResponse> Reviews { get; set; } = new List<ReviewResponse>();
        public bool UserHasReviewed { get; set; }
        public bool UserCanReview { get; set; } // True if user has book on "Read" shelf and hasn't reviewed yet
    }

    public class CreateReviewResponse
    {
        public string Message { get; set; }
        public ReviewResponse Review { get; set; }
    }

    public class UpdateReviewResponse
    {
        public string Message { get; set; }
        public ReviewResponse Review { get; set; }
    }

    public class CreateCommentResponse
    {
        public string Message { get; set; }
        public CommentResponse Comment { get; set; }
    }

    public class UpdateCommentResponse
    {
        public string Message { get; set; }
        public CommentResponse Comment { get; set; }
    }

    public class ReviewDeleteResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
    }

    public class UserReviewsResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string UserFullName { get; set; }
        public List<ReviewResponse> Reviews { get; set; } = new List<ReviewResponse>();
        public int TotalReviews { get; set; }
        public decimal AverageRating { get; set; }
    }
}