namespace Persistence.Models
{
    public class InventoryTransaction : BaseEntity
    {
        public int InventoryItemId { get; set; }
        public int QuantityChange { get; set; }
        public DateTime TransactionDate { get; set; }
        public int StaffUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public User StaffUser { get; set; } = null!;
    }
}
