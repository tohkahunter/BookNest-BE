using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Request.BookShelf
{
    public class UpdateShelfRequest
    {
        [Required]
        public int ShelfId { get; set; }

        [Required]
        [MaxLength(100, ErrorMessage = "Shelf name cannot exceed 100 characters")]
        public string ShelfName { get; set; }

        [MaxLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
