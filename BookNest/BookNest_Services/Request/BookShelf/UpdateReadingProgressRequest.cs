using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Request.BookShelf
{
    public class UpdateReadingProgressRequest
    {
        [Required]
        public int BookId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current page must be a positive number")]
        public int? CurrentPage { get; set; }

        [Range(0, 100, ErrorMessage = "Reading progress must be between 0 and 100")]
        public decimal? ReadingProgress { get; set; }

        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }
    }
}
