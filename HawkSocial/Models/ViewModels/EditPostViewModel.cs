using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace HawkSocial.Models
{
    public class EditPostViewModel
    {
        [Required]
        [MaxLength(140)]
        public string Content { get; set; } = string.Empty;
    }


}
