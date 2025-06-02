using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Request.BookShelf
{
    public class MoveBookToShelfRequest
    {
        [Required]
        public int BookId { get; set; }

        public int? NewShelfId { get; set; } // null means remove from shelf
    }
}
