using System.ComponentModel.DataAnnotations;

namespace HermesChatApp.Models
{
    public class GroupModel
    {
        [Key]
        public int GroupId { get; set; }
        [MaxLength(50)]
        public string? GroupName { get; set; }
    }
}
