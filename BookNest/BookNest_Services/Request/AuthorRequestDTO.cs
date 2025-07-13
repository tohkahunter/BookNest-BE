using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookNest_Services.Request
{
    public class CreateAuthorRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string Name { get; set; }
    }

    public class UpdateAuthorRequest
    {
        [Required]
        public int AuthorId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters")]
        public string Name { get; set; }
    }
}
