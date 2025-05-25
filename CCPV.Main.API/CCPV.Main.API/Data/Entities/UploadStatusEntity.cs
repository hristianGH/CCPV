using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class UploadStatusEntity
    {
        [Key]
        public string UploadId { get; set; } = null!;

        public string Status { get; set; }

        public string? Checksum { get; set; }

        public string? Message { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }
    }
}
