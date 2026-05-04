namespace Persistence.Models
{
    public class PriceAdjustment : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal PercentageModifier { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCumulative { get; set; }
    }
}
