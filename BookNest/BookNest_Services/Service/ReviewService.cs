using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using BookNest_Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookNest_Services.Service
{
    public class ReviewService : IReviewService
    {
        private readonly BookTracker7Context _context;
        private readonly IUserBooksRepository _userBooksRepository;

        public ReviewService(BookTracker7Context context, IUserBooksRepository userBooksRepository)
        {
            _context = context;
            _userBooksRepository = userBooksRepository;
        }

        #region Review CRUD Operations

        public async Task<Review> CreateReviewAsync(int userId, int bookId, string reviewText, int rating, bool isPublic = true)
        {
            //// Check if user can review this book (must have it on "Read" shelf)
            //if (!await CanUserReviewBookAsync(userId, bookId))
            //    throw new InvalidOperationException("You can only review books that you have marked as 'Read'");

            //// Check if user has already reviewed this book
            //if (await HasUserReviewedBookAsync(userId, bookId))
            //    throw new InvalidOperationException("You have already reviewed this book");

            var review = new Review
            {
                UserId = userId,
                BookId = bookId,
                ReviewText = reviewText.Trim(),
                Rating = rating,
                IsPublic = isPublic,
                DateReviewed = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(review)
                .Reference(r => r.User)
                .LoadAsync();
            await _context.Entry(review)
                .Reference(r => r.Book)
                .LoadAsync();

            return review;
        }

        public async Task<Review> UpdateReviewAsync(int userId, int reviewId, string reviewText, int rating, bool isPublic = true)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                throw new InvalidOperationException("Review not found");

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own reviews");

            review.ReviewText = reviewText.Trim();
            review.Rating = rating;
            review.IsPublic = isPublic;

            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(review)
                .Reference(r => r.User)
                .LoadAsync();
            await _context.Entry(review)
                .Reference(r => r.Book)
                .LoadAsync();

            return review;
        }

        public async Task<bool> DeleteReviewAsync(int userId, int reviewId, bool isAdmin = false)
        {
            var review = await _context.Reviews
                .Include(r => r.Comments)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
                return false;

            if (!isAdmin && review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews");

            // Delete all comments first
            if (review.Comments.Any())
            {
                _context.Comments.RemoveRange(review.Comments);
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Review> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .Include(r => r.Comments)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<Review> GetUserReviewForBookAsync(int userId, int bookId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        #endregion

        #region Review Queries

        public async Task<IEnumerable<Review>> GetReviewsForBookAsync(int bookId, bool publicOnly = true)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.BookId == bookId);

            if (publicOnly)
                query = query.Where(r => r.IsPublic);

            return await query
                .OrderByDescending(r => r.DateReviewed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetUserReviewsAsync(int userId, bool publicOnly = true)
        {
            var query = _context.Reviews
                .Include(r => r.Book)
                    .ThenInclude(b => b.Author)
                .Include(r => r.User)
                .Where(r => r.UserId == userId);

            if (publicOnly)
                query = query.Where(r => r.IsPublic);

            return await query
                .OrderByDescending(r => r.DateReviewed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10, bool publicOnly = true)
        {
            var reviewQuery = _context.Reviews.AsQueryable();

            if (publicOnly)
                reviewQuery = reviewQuery.Where(r => r.IsPublic);

            var reviews = await reviewQuery
                .OrderByDescending(r => r.DateReviewed)
                .Take(count)
                .ToListAsync();

            // Load navigation properties separately to avoid EF translation issues
            foreach (var review in reviews)
            {
                await _context.Entry(review)
                    .Reference(r => r.User)
                    .LoadAsync();

                await _context.Entry(review)
                    .Reference(r => r.Book)
                    .LoadAsync();

                if (review.Book != null)
                {
                    await _context.Entry(review.Book)
                        .Reference(b => b.Author)
                        .LoadAsync();
                }
            }

            return reviews;
        }

        #endregion

        #region Review Statistics

        public async Task<decimal> GetAverageRatingForBookAsync(int bookId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.BookId == bookId && r.IsPublic)
                .ToListAsync();

            return reviews.Any() ? (decimal)reviews.Average(r => r.Rating) : 0;
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionForBookAsync(int bookId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.BookId == bookId && r.IsPublic)
                .ToListAsync();

            var distribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                distribution[i] = reviews.Count(r => r.Rating == i);
            }

            return distribution;
        }

        public async Task<int> GetReviewCountForBookAsync(int bookId)
        {
            return await _context.Reviews
                .CountAsync(r => r.BookId == bookId && r.IsPublic);
        }

        #endregion

        #region User Permissions

        public async Task<bool> CanUserReviewBookAsync(int userId, int bookId)
        {
            // User must have the book on their "Read" shelf (StatusId = 3)
            return await _userBooksRepository.ExistsAsync(ub =>
                ub.UserId == userId &&
                ub.BookId == bookId &&
                ub.StatusId == 3);
        }

        public async Task<bool> HasUserReviewedBookAsync(int userId, int bookId)
        {
            return await _context.Reviews.AnyAsync(r => r.UserId == userId && r.BookId == bookId);
        }

        #endregion

        #region Comment Operations

        public async Task<Comment> CreateCommentAsync(int userId, int reviewId, string commentText)
        {
            // Verify review exists and is public
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                throw new InvalidOperationException("Review not found");

            if (!review.IsPublic)
                throw new InvalidOperationException("Cannot comment on private reviews");

            var comment = new Comment
            {
                ReviewId = reviewId,
                UserId = userId,
                CommentText = commentText.Trim(),
                DateCommented = DateTime.UtcNow,
                IsEdited = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(comment)
                .Reference(c => c.User)
                .LoadAsync();

            return comment;
        }

        public async Task<Comment> UpdateCommentAsync(int userId, int commentId, string commentText)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own comments");

            comment.CommentText = commentText.Trim();
            comment.IsEdited = true;
            comment.EditedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(comment)
                .Reference(c => c.User)
                .LoadAsync();

            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int userId, int commentId, bool isAdmin = false)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            if (!isAdmin && comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Comment>> GetCommentsForReviewAsync(int reviewId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ReviewId == reviewId)
                .OrderBy(c => c.DateCommented)
                .ToListAsync();
        }

        #endregion

        #region Enhanced Queries

        public async Task<IEnumerable<Review>> GetReviewsWithDetailsAsync(int bookId, int? currentUserId = null)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.User)
                .Where(r => r.BookId == bookId && r.IsPublic)
                .OrderByDescending(r => r.DateReviewed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsWithDetailsAsync(int reviewId, int? currentUserId = null)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.ReviewId == reviewId)
                .OrderBy(c => c.DateCommented)
                .ToListAsync();
        }

        #endregion
    }
}