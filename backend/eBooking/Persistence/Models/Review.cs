namespace Persistence.Models
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; }
        public bool IsVerified { get; set; } = false;
        public bool IsApproved { get; set; } = true; 
        public int HotelId { get; set; }
        public int? UserId { get; set; }
        public int? BookingId { get; set; }

        // Audit trag moderacije (ko je i kada odobrio/odbio recenziju) — isti princip kao
        // SupportTicket.RespondedByUserId/RespondedAt. Do sada su approvedByUserId/rejectedByUserId
        // parametri u ReviewService.Approve/RejectReviewAsync postojali, ali se nigdje nisu čuvali.
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }

        public Hotel Hotel { get; set; } = null!;
        public User? User { get; set; }
        public Booking? Booking { get; set; }
        public User? ApprovedByUser { get; set; }
        public User? RejectedByUser { get; set; }
    }

}
