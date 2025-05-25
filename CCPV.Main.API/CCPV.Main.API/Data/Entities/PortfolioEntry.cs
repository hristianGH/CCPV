using System.ComponentModel.DataAnnotations;

namespace CCPV.Main.API.Data.Entities
{
    public class PortfolioEntry
    {
        public Guid Id { get; set; }

        [Required]
        public string CoinSymbol { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public decimal BuyPrice { get; set; }

        [Required]
        public Guid PortfolioId { get; set; }

        [Required]
        public Portfolio Portfolio { get; set; } = null!;
    }
    s
}
