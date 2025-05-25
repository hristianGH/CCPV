
using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        public ICollection<PortfolioEntity> Portfolios { get; set; } = [];
    }
}
