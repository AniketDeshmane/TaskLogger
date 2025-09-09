using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskLogger.Models
{
    [Table("Tasks")]
    public class TaskEntry
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Task { get; set; } = "";
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [MaxLength(50)]
        public string? EventType { get; set; } // e.g., "SHUTDOWN", "SUSPEND", or null for manual
        
        [MaxLength(100)]
        public string? Notes { get; set; }
        
        // Computed property for backward compatibility
        [NotMapped]
        public string Timestamp => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        
        [NotMapped]
        public DateTime DateTime => CreatedAt;
    }
}
