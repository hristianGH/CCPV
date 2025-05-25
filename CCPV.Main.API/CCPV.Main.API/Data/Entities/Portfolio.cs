using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class Portfolio
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public User User { get; set; } = null!;

        public ICollection<PortfolioEntry> Entries { get; set; } = [];
    }

}
