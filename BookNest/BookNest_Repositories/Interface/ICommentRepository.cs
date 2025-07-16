using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Repositories.Interface
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        // Add any specific comment repository methods here if needed
    }
}
