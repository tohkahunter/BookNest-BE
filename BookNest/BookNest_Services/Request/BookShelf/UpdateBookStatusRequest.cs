using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Request.BookShelf
{
    public class UpdateBookStatusRequest
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [Range(1, 3, ErrorMessage = "StatusId must be between 1 and 3 (1: Want to Read, 2: Currently Reading, 3: Read)")]
        public int NewStatusId { get; set; }
    }
}
