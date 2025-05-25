
using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        public ICollection<Portfolio> Portfolios { get; set; } = [];
    }
}
