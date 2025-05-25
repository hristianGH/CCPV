using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class PortfolioEntity
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public UserEntity User { get; set; } = null!;

        public ICollection<PortfolioEntryEntity> Entries { get; set; } = [];
    }

}
