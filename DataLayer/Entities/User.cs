using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? Email { get; set; }
    }
}