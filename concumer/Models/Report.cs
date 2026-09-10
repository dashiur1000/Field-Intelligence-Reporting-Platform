using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace concumer.Models
{
    public class Report
    {
        [Required]
        public string reportId { get; set; }

        [Required]
        [JsonPropertyName("@timestamp")]
        public DateTime timestamp { get; set; }

        [Required]
        public DateTime processedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string agentId { get; set; }
        [Required]
        public string unit { get; set; }
        [Required]
        public string theater { get; set; }
        [Required]
        public string sector { get; set; }
        [Required]
        public string location { get; set; }
        [Required]
        public string reportType { get; set; }
        [Required]
        public string priority { get; set; }
        [Required]
        public string sourceType { get; set; }
        [Required]
        public string message { get; set; }
        public string? subjectId { get; set; }
        public string? subjectType { get; set; }
    }
}