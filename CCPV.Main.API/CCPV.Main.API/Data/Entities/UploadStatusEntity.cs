using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class UploadStatusEntity
    {
        [Key]
        public string UploadId { get; set; } = null!;

        [Required]
        public string Status { get; set; }

        public string? Checksum { get; set; }

        public string? Message { get; set; }

        public int TotalChunks { get; set; }

        public string? FilePath { get; set; } = null!;

        [Required]
        public DateTime LastUpdated { get; set; }
    }
}
