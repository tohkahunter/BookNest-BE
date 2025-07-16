using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Repositories.Interface
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        // Add any specific review repository methods here if needed
    }
}
