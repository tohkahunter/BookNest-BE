using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Repositories.Repository
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        public CommentRepository(BookTracker7Context context) : base(context)
        {
        }
        // Add any specific methods for CommentRepository here if needed
    }
}
