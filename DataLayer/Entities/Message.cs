using System.ComponentModel.DataAnnotations;

namespace DataLayer
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }
        public User? MessageFrom { get; set; }
        public User? MessageToUser { get; set; }
        public Group? MessageToGroup { get; set; }
        public string? MessageContent { get; set; }
        public DateTime MessageTime { get; set; }
    }
}
