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
        [Required(ErrorMessage = "ID artikla je obavezan.")]
        public int InventoryItemId { get; set; }

        [Required(ErrorMessage = "Promjena količine je obavezna.")]
        public int QuantityChange { get; set; }

        [Required(ErrorMessage = "Datum transakcije je obavezan.")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "ID zaposlenika je obavezan.")]
        public int StaffUserId { get; set; }

        [Required(ErrorMessage = "Razlog je obavezan.")]
        [StringLength(500, ErrorMessage = "Razlog ne smije imati više od 500 karaktera.")]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateInventoryTransactionDto : UpdateBaseEntityDto
    {
        [Required(ErrorMessage = "ID artikla je obavezan.")]
        public int InventoryItemId { get; set; }

        [Required(ErrorMessage = "Promjena količine je obavezna.")]
        public int QuantityChange { get; set; }

        [Required(ErrorMessage = "Datum transakcije je obavezan.")]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "ID zaposlenika je obavezan.")]
        public int StaffUserId { get; set; }

        [Required(ErrorMessage = "Razlog je obavezan.")]
        [StringLength(500, ErrorMessage = "Razlog ne smije imati više od 500 karaktera.")]
        public string Reason { get; set; } = string.Empty;
    }
}
