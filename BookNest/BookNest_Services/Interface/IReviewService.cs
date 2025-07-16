using BookNest_Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookNest_Services.Interface
{
    public interface IReviewService
    {
        // Review CRUD operations
        Task<Review> CreateReviewAsync(int userId, int bookId, string reviewText, int rating, bool isPublic = true);
        Task<Review> UpdateReviewAsync(int userId, int reviewId, string reviewText, int rating, bool isPublic = true);
        Task<bool> DeleteReviewAsync(int userId, int reviewId, bool isAdmin = false);
        Task<Review> GetReviewByIdAsync(int reviewId);
        Task<Review> GetUserReviewForBookAsync(int userId, int bookId);

        // Review queries
        Task<IEnumerable<Review>> GetReviewsForBookAsync(int bookId, bool publicOnly = true);
        Task<IEnumerable<Review>> GetUserReviewsAsync(int userId, bool publicOnly = true);
        Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10, bool publicOnly = true);

        // Review statistics
        Task<decimal> GetAverageRatingForBookAsync(int bookId);
        Task<Dictionary<int, int>> GetRatingDistributionForBookAsync(int bookId);
        Task<int> GetReviewCountForBookAsync(int bookId);

        // User permissions
        Task<bool> CanUserReviewBookAsync(int userId, int bookId);
        Task<bool> HasUserReviewedBookAsync(int userId, int bookId);

        // Comment operations
        Task<Comment> CreateCommentAsync(int userId, int reviewId, string commentText);
        Task<Comment> UpdateCommentAsync(int userId, int commentId, string commentText);
        Task<bool> DeleteCommentAsync(int userId, int commentId, bool isAdmin = false);
        Task<IEnumerable<Comment>> GetCommentsForReviewAsync(int reviewId);

        // Enhanced queries with navigation properties
        Task<IEnumerable<Review>> GetReviewsWithDetailsAsync(int bookId, int? currentUserId = null);
        Task<IEnumerable<Comment>> GetCommentsWithDetailsAsync(int reviewId, int? currentUserId = null);
    }
}