using System.ComponentModel.DataAnnotations;

namespace DataLayer.Entities
{
    public class GroupMember
    {
        [Key]
        public int Id { get; set; }
        public Group? Group { get; set; }
        public User? User { get; set; }
    }
}
