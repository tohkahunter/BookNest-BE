using BookNest_Repositories.Interface;
using BookNest_Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Repositories.Repository
{
    public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
    {
        public AuthorRepository(BookTracker7Context context) : base(context)
        {
        }
        // You can add any specific methods for AuthorRepository here if needed
        // For example, methods to find authors by name, etc.
    }
}
