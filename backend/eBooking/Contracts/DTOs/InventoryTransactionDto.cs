using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class InventoryTransactionDto : BaseEntityDto
    {
        public int InventoryItemId { get; set; }
        public string InventoryItemName { get; set; } = string.Empty;
        public string InventoryItemUnit { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public DateTime TransactionDate { get; set; }
        public int StaffUserId { get; set; }
        public string StaffUserName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class CreateInventoryTransactionDto : CreateBaseEntityDto
    {
        [Required(ErrorMessage = "Inventory item ID is required")]
        public int InventoryItemId { get; set; }

        [Required(ErrorMessage = "Quantity change is required")]
        public int QuantityChange { get; set; }

        [Required(ErrorMessage = "Transaction date is required")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Staff user ID is required")]
        public int StaffUserId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateInventoryTransactionDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "Inventory item ID is required")]
        public int InventoryItemId { get; set; }

        [Required(ErrorMessage = "Quantity change is required")]
        public int QuantityChange { get; set; }

        [Required(ErrorMessage = "Transaction date is required")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Staff user ID is required")]
        public int StaffUserId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
