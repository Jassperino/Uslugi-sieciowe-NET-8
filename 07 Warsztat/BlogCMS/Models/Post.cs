using System.ComponentModel.DataAnnotations;

namespace BlogCMS.Models
{
    public class Post
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(20000)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime Published { get; set; }
    }
}
