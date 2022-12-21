using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        [MaxLength(50)]
        public string? GroupName { get; set; }
    }
}
