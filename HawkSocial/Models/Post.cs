using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace HawkSocial.Models
{
    public class Post
    {
        // KEY //
        public int Id { get; set; }

        // DATA //
        [Required, MaxLength(140)]
        public string Content { get; set; } = string.Empty;

        // METADATA //
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public bool IsEdited { get; set; } = false;

        // FK //
        [Required]
        public string UserId { get; set; } = default!;
        public IdentityUser User { get; set; } = default!;
    }
}
