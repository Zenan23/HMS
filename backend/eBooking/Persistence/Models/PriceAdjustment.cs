namespace Persistence.Models
{
    public class PriceAdjustment : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal PercentageModifier { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCumulative { get; set; }

        /// <summary>Admin/employee koji je kreirao pravilo (audit). Server-side se popunjava iz JWT-a.</summary>
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        /// <summary>Null = važi za sve hotele (sajt-wide). Popunjeno = važi samo za taj hotel.</summary>
        public int? HotelId { get; set; }
        public Hotel? Hotel { get; set; }
    }
}
