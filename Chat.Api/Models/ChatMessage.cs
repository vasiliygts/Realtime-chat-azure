using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chat.Api.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        [MaxLength(20)]
        public string? Sentiment { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? SentimentScore { get; set; }
    }
}