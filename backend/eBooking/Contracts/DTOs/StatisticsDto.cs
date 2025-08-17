using System.ComponentModel.DataAnnotations;

namespace Contracts.DTOs
{
    public class DashboardStatistics
    {
        public PaymentStatistics PaymentStats { get; set; } = new();
        public BookingStatistics BookingStats { get; set; } = new();
        public HotelStatistics HotelStats { get; set; } = new();
        public UserStatistics UserStats { get; set; } = new();
        public ReviewStatistics ReviewStats { get; set; } = new();
    }

    public class PaymentStatistics
    {
        public decimal TotalPayments { get; set; }
        public decimal TotalRefunds { get; set; }
        public decimal NetPayments { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<MonthlyPaymentData> MonthlyData { get; set; } = new();
    }

    public class BookingStatistics
    {
        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageBookingValue { get; set; }
        public double AverageOccupancyRate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<MonthlyBookingData> MonthlyData { get; set; } = new();
    }

    public class HotelStatistics
    {
        public int TotalHotels { get; set; }
        public int ActiveHotels { get; set; }
        public double AverageRating { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public List<TopHotelData> TopHotels { get; set; } = new();
        public List<HotelOccupancyData> OccupancyData { get; set; } = new();
    }

    public class UserStatistics
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int GuestUsers { get; set; }
        public int EmployeeUsers { get; set; }
        public int AdminUsers { get; set; }
        public List<MonthlyUserData> MonthlyData { get; set; } = new();
    }

    public class ReviewStatistics
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarReviews { get; set; }
        public int FourStarReviews { get; set; }
        public int ThreeStarReviews { get; set; }
        public int TwoStarReviews { get; set; }
        public int OneStarReviews { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<MonthlyReviewData> MonthlyData { get; set; } = new();
    }

    public class MonthlyPaymentData
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class MonthlyBookingData
    {
        public string Month { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class TopHotelData
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public double OccupancyRate { get; set; }
    }

    public class HotelOccupancyData
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public double OccupancyRate { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
    }

    public class MonthlyUserData
    {
        public string Month { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class MonthlyReviewData
    {
        public string Month { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }
}
