using BookNest_Services.Interface;
using BookNest_Services.Request.Review;
using BookNest_Services.Request.Comment;
using BookNest_Services.Response.Review;
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
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        #region Review Management

        /// <summary>
        /// Create a review for a book (User must have book on "Read" shelf)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CreateReviewResponse>> CreateReview([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            try
            {
                var review = await _reviewService.CreateReviewAsync(
                    userId, request.BookId, request.ReviewText, request.Rating, request.IsPublic);

                var response = new CreateReviewResponse
                {
                    Message = "Review created successfully",
                    Review = new ReviewResponse
                    {
                        ReviewId = review.ReviewId,
                        UserId = review.UserId,
                        Username = review.User?.Username,
                        UserFullName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                        UserProfilePictureUrl = review.User?.ProfilePictureUrl,
                        BookId = review.BookId,
                        BookTitle = review.Book?.Title,
                        ReviewText = review.ReviewText,
                        Rating = review.Rating,
                        DateReviewed = review.DateReviewed,
                        IsPublic = review.IsPublic,
                        CommentCount = 0,
                        CanEdit = true,
                        CanDelete = true
                    }
                };

                return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewId }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing review
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateReviewResponse>> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != request.ReviewId)
                return BadRequest("ID in URL does not match ID in request body");

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            try
            {
                var review = await _reviewService.UpdateReviewAsync(
                    userId, request.ReviewId, request.ReviewText, request.Rating, request.IsPublic);

                var response = new UpdateReviewResponse
                {
                    Message = "Review updated successfully",
                    Review = new ReviewResponse
                    {
                        ReviewId = review.ReviewId,
                        UserId = review.UserId,
                        Username = review.User?.Username,
                        UserFullName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                        UserProfilePictureUrl = review.User?.ProfilePictureUrl,
                        BookId = review.BookId,
                        BookTitle = review.Book?.Title,
                        ReviewText = review.ReviewText,
                        Rating = review.Rating,
                        DateReviewed = review.DateReviewed,
                        IsPublic = review.IsPublic,
                        CommentCount = review.Comments?.Count() ?? 0,
                        CanEdit = true,
                        CanDelete = true
                    }
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ReviewDeleteResponse>> DeleteReview(int id)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            try
            {
                var success = await _reviewService.DeleteReviewAsync(userId, id, IsAdmin());
                if (!success)
                {
                    return NotFound(new ReviewDeleteResponse
                    {
                        Success = false,
                        Message = "Review not found"
                    });
                }

                return Ok(new ReviewDeleteResponse
                {
                    Success = true,
                    Message = "Review deleted successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ReviewDeleteResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get a specific review by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound("Review not found");

            if (!review.IsPublic)
            {
                var currentUserId = GetUserId();
                if (currentUserId != review.UserId && !IsAdmin())
                    return NotFound("Review not found");
            }

            var currentUser = GetUserId();
            var response = new ReviewResponse
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                Username = review.User?.Username,
                UserFullName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                UserProfilePictureUrl = review.User?.ProfilePictureUrl,
                BookId = review.BookId,
                BookTitle = review.Book?.Title,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                DateReviewed = review.DateReviewed,
                IsPublic = review.IsPublic,
                CommentCount = review.Comments?.Count() ?? 0,
                CanEdit = currentUser == review.UserId,
                CanDelete = currentUser == review.UserId || IsAdmin()
            };

            return Ok(response);
        }

        #endregion

        #region Book Reviews

        /// <summary>
        /// Get all reviews for a specific book with rating statistics
        /// </summary>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<BookReviewSummaryResponse>> GetBookReviews(int bookId)
        {
            var reviews = await _reviewService.GetReviewsWithDetailsAsync(bookId, GetUserId());
            var averageRating = await _reviewService.GetAverageRatingForBookAsync(bookId);
            var ratingDistribution = await _reviewService.GetRatingDistributionForBookAsync(bookId);

            var currentUserId = GetUserId();
            var userHasReviewed = currentUserId > 0 && await _reviewService.HasUserReviewedBookAsync(currentUserId, bookId);
            var userCanReview = currentUserId > 0 && !userHasReviewed && await _reviewService.CanUserReviewBookAsync(currentUserId, bookId);

            var reviewResponses = reviews.Select(r => new ReviewResponse
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                Username = r.User?.Username,
                UserFullName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                UserProfilePictureUrl = r.User?.ProfilePictureUrl,
                BookId = r.BookId,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                DateReviewed = r.DateReviewed,
                IsPublic = r.IsPublic,
                CommentCount = r.Comments?.Count() ?? 0,
                CanEdit = currentUserId == r.UserId,
                CanDelete = currentUserId == r.UserId || IsAdmin()
            }).ToList();

            var response = new BookReviewSummaryResponse
            {
                BookId = bookId,
                BookTitle = reviews.FirstOrDefault()?.Book?.Title,
                AverageRating = averageRating,
                TotalReviews = reviewResponses.Count,
                FiveStarCount = ratingDistribution[5],
                FourStarCount = ratingDistribution[4],
                ThreeStarCount = ratingDistribution[3],
                TwoStarCount = ratingDistribution[2],
                OneStarCount = ratingDistribution[1],
                Reviews = reviewResponses,
                UserHasReviewed = userHasReviewed,
                UserCanReview = userCanReview
            };

            return Ok(response);
        }

        /// <summary>
        /// Check if current user can review a specific book
        /// </summary>
        [HttpGet("book/{bookId}/can-review")]
        public async Task<ActionResult<bool>> CanUserReviewBook(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var hasReviewed = await _reviewService.HasUserReviewedBookAsync(userId, bookId);
            if (hasReviewed)
                return Ok(false);

            var canReview = await _reviewService.CanUserReviewBookAsync(userId, bookId);
            return Ok(canReview);
        }

        /// <summary>
        /// Get current user's review for a specific book
        /// </summary>
        [HttpGet("book/{bookId}/my-review")]
        public async Task<ActionResult<ReviewResponse>> GetMyReviewForBook(int bookId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var review = await _reviewService.GetUserReviewForBookAsync(userId, bookId);
            if (review == null)
                return NotFound("You have not reviewed this book");

            var response = new ReviewResponse
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                Username = review.User?.Username,
                UserFullName = $"{review.User?.FirstName} {review.User?.LastName}".Trim(),
                UserProfilePictureUrl = review.User?.ProfilePictureUrl,
                BookId = review.BookId,
                BookTitle = review.Book?.Title,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                DateReviewed = review.DateReviewed,
                IsPublic = review.IsPublic,
                CommentCount = review.Comments?.Count() ?? 0,
                CanEdit = true,
                CanDelete = true
            };

            return Ok(response);
        }

        #endregion

        #region User Reviews

        /// <summary>
        /// Get all reviews by a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserReviewsResponse>> GetUserReviews(int userId)
        {
            var reviews = await _reviewService.GetUserReviewsAsync(userId, publicOnly: true);
            var currentUserId = GetUserId();

            var reviewResponses = reviews.Select(r => new ReviewResponse
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                Username = r.User?.Username,
                UserFullName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                UserProfilePictureUrl = r.User?.ProfilePictureUrl,
                BookId = r.BookId,
                BookTitle = r.Book?.Title,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                DateReviewed = r.DateReviewed,
                IsPublic = r.IsPublic,
                CommentCount = r.Comments?.Count() ?? 0,
                CanEdit = currentUserId == r.UserId,
                CanDelete = currentUserId == r.UserId || IsAdmin()
            }).ToList();

            var firstReview = reviews.FirstOrDefault();
            var response = new UserReviewsResponse
            {
                UserId = userId,
                Username = firstReview?.User?.Username,
                UserFullName = $"{firstReview?.User?.FirstName} {firstReview?.User?.LastName}".Trim(),
                Reviews = reviewResponses,
                TotalReviews = reviewResponses.Count,
                AverageRating = reviewResponses.Any() ? (decimal)reviewResponses.Average(r => r.Rating) : 0
            };

            return Ok(response);
        }

        /// <summary>
        /// Get current user's reviews
        /// </summary>
        [HttpGet("my-reviews")]
        public async Task<ActionResult<UserReviewsResponse>> GetMyReviews()
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            var reviews = await _reviewService.GetUserReviewsAsync(userId, publicOnly: false);

            var reviewResponses = reviews.Select(r => new ReviewResponse
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                Username = r.User?.Username,
                UserFullName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                UserProfilePictureUrl = r.User?.ProfilePictureUrl,
                BookId = r.BookId,
                BookTitle = r.Book?.Title,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                DateReviewed = r.DateReviewed,
                IsPublic = r.IsPublic,
                CommentCount = r.Comments?.Count() ?? 0,
                CanEdit = true,
                CanDelete = true
            }).ToList();

            var firstReview = reviews.FirstOrDefault();
            var response = new UserReviewsResponse
            {
                UserId = userId,
                Username = firstReview?.User?.Username,
                UserFullName = $"{firstReview?.User?.FirstName} {firstReview?.User?.LastName}".Trim(),
                Reviews = reviewResponses,
                TotalReviews = reviewResponses.Count,
                AverageRating = reviewResponses.Any() ? (decimal)reviewResponses.Average(r => r.Rating) : 0
            };

            return Ok(response);
        }

        #endregion

        #region Recent Reviews

        /// <summary>
        /// Get recent reviews across all books
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetRecentReviews([FromQuery] int count = 10)
        {
            var reviews = await _reviewService.GetRecentReviewsAsync(count, publicOnly: true);
            var currentUserId = GetUserId();

            var response = reviews.Select(r => new ReviewResponse
            {
                ReviewId = r.ReviewId,
                UserId = r.UserId,
                Username = r.User?.Username,
                UserFullName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                UserProfilePictureUrl = r.User?.ProfilePictureUrl,
                BookId = r.BookId,
                BookTitle = r.Book?.Title,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                DateReviewed = r.DateReviewed,
                IsPublic = r.IsPublic,
                CommentCount = r.Comments?.Count() ?? 0,
                CanEdit = currentUserId == r.UserId,
                CanDelete = currentUserId == r.UserId || IsAdmin()
            });

            return Ok(response);
        }

        #endregion

        #region Comment Management

        /// <summary>
        /// Create a comment on a review
        /// </summary>
        [HttpPost("{reviewId}/comments")]
        public async Task<ActionResult<CreateCommentResponse>> CreateComment(int reviewId, [FromBody] CreateCommentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (reviewId != request.ReviewId)
                return BadRequest("Review ID in URL does not match request body");

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            try
            {
                var comment = await _reviewService.CreateCommentAsync(userId, request.ReviewId, request.CommentText);

                var response = new CreateCommentResponse
                {
                    Message = "Comment created successfully",
                    Comment = new CommentResponse
                    {
                        CommentId = comment.CommentId,
                        ReviewId = comment.ReviewId,
                        UserId = comment.UserId,
                        Username = comment.User?.Username,
                        UserFullName = $"{comment.User?.FirstName} {comment.User?.LastName}".Trim(),
                        UserProfilePictureUrl = comment.User?.ProfilePictureUrl,
                        CommentText = comment.CommentText,
                        DateCommented = comment.DateCommented,
                        IsEdited = comment.IsEdited,
                        EditedAt = comment.EditedAt,
                        CanEdit = true,
                        CanDelete = true
                    }
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update a comment
        /// </summary>
        [HttpPut("comments/{commentId}")]
        public async Task<ActionResult<UpdateCommentResponse>> UpdateComment(int commentId, [FromBody] UpdateCommentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (commentId != request.CommentId)
                return BadRequest("Comment ID in URL does not match request body");

            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            try
            {
                var comment = await _reviewService.UpdateCommentAsync(userId, request.CommentId, request.CommentText);

                var response = new UpdateCommentResponse
                {
                    Message = "Comment updated successfully",
                    Comment = new CommentResponse
                    {
                        CommentId = comment.CommentId,
                        ReviewId = comment.ReviewId,
                        UserId = comment.UserId,
                        Username = comment.User?.Username,
                        UserFullName = $"{comment.User?.FirstName} {comment.User?.LastName}".Trim(),
                        UserProfilePictureUrl = comment.User?.ProfilePictureUrl,
                        CommentText = comment.CommentText,
                        DateCommented = comment.DateCommented,
                        IsEdited = comment.IsEdited,
                        EditedAt = comment.EditedAt,
                        CanEdit = true,
                        CanDelete = true
                    }
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a comment
        /// </summary>
        [HttpDelete("comments/{commentId}")]
        public async Task<ActionResult<ReviewDeleteResponse>> DeleteComment(int commentId)
        {
            var userId = GetUserId();
            if (userId == 0)
                return Unauthorized("Invalid user");

            try
            {
                var success = await _reviewService.DeleteCommentAsync(userId, commentId, IsAdmin());
                if (!success)
                {
                    return NotFound(new ReviewDeleteResponse
                    {
                        Success = false,
                        Message = "Comment not found"
                    });
                }

                return Ok(new ReviewDeleteResponse
                {
                    Success = true,
                    Message = "Comment deleted successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ReviewDeleteResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all comments for a review
        /// </summary>
        [HttpGet("{reviewId}/comments")]
        public async Task<ActionResult<IEnumerable<CommentResponse>>> GetReviewComments(int reviewId)
        {
            var comments = await _reviewService.GetCommentsWithDetailsAsync(reviewId, GetUserId());
            var currentUserId = GetUserId();

            var response = comments.Select(c => new CommentResponse
            {
                CommentId = c.CommentId,
                ReviewId = c.ReviewId,
                UserId = c.UserId,
                Username = c.User?.Username,
                UserFullName = $"{c.User?.FirstName} {c.User?.LastName}".Trim(),
                UserProfilePictureUrl = c.User?.ProfilePictureUrl,
                CommentText = c.CommentText,
                DateCommented = c.DateCommented,
                IsEdited = c.IsEdited,
                EditedAt = c.EditedAt,
                CanEdit = currentUserId == c.UserId,
                CanDelete = currentUserId == c.UserId || IsAdmin()
            });

            return Ok(response);
        }

        #endregion
    }
}