class DashboardStatistics {
  final PaymentStatistics paymentStats;
  final BookingStatistics bookingStats;
  final HotelStatistics hotelStats;
  final UserStatistics userStats;
  final ReviewStatistics reviewStats;

  DashboardStatistics({
    required this.paymentStats,
    required this.bookingStats,
    required this.hotelStats,
    required this.userStats,
    required this.reviewStats,
  });

  factory DashboardStatistics.fromJson(Map<String, dynamic> json) {
    return DashboardStatistics(
      paymentStats: PaymentStatistics.fromJson(json['paymentStats']),
      bookingStats: BookingStatistics.fromJson(json['bookingStats']),
      hotelStats: HotelStatistics.fromJson(json['hotelStats']),
      userStats: UserStatistics.fromJson(json['userStats']),
      reviewStats: ReviewStatistics.fromJson(json['reviewStats']),
    );
  }
}

class PaymentStatistics {
  final double totalPayments;
  final double netPayments;
  final double pendingPayments;
  final double cancelledPayments;
  final List<MonthlyPaymentData> monthlyData;

  PaymentStatistics({
    required this.totalPayments,
    required this.netPayments,
    required this.pendingPayments,
    required this.cancelledPayments,
    required this.monthlyData,
  });

  factory PaymentStatistics.fromJson(Map<String, dynamic> json) {
    return PaymentStatistics(
      totalPayments: json['totalPayments']?.toDouble() ?? 0.0,
      netPayments: json['netPayments']?.toDouble() ?? 0.0,
      pendingPayments: json['pendingPayments']?.toDouble() ?? 0.0,
      cancelledPayments: json['cancelledPayments']?.toDouble() ?? 0.0,
      monthlyData: (json['monthlyData'] as List?)
          ?.map((e) => MonthlyPaymentData.fromJson(e))
          .toList() ?? [],
    );
  }
}

class MonthlyPaymentData {
  final double totalAmount;
  final int count;

  MonthlyPaymentData({
    required this.totalAmount,
    required this.count,
  });

  factory MonthlyPaymentData.fromJson(Map<String, dynamic> json) {
    return MonthlyPaymentData(
      totalAmount: json['totalAmount']?.toDouble() ?? 0.0,
      count: json['count'] ?? 0,
    );
  }
}

class BookingStatistics {
  final int totalBookings;
  final int confirmedBookings;
  final int pendingBookings;
  final int cancelledBookings;
  final int completedBookings;
  final double totalRevenue;

  BookingStatistics({
    required this.totalBookings,
    required this.confirmedBookings,
    required this.pendingBookings,
    required this.cancelledBookings,
    required this.completedBookings,
    required this.totalRevenue,
  });

  factory BookingStatistics.fromJson(Map<String, dynamic> json) {
    return BookingStatistics(
      totalBookings: json['totalBookings'] ?? 0,
      confirmedBookings: json['confirmedBookings'] ?? 0,
      pendingBookings: json['pendingBookings'] ?? 0,
      cancelledBookings: json['cancelledBookings'] ?? 0,
      completedBookings: json['completedBookings'] ?? 0,
      totalRevenue: json['totalRevenue']?.toDouble() ?? 0.0,
    );
  }
}

class HotelStatistics {
  final int totalHotels;
  final int activeHotels;
  final double averageRating;
  final int totalRooms;
  final int availableRooms;
  final List<TopHotelData> topHotels;
  final List<HotelOccupancyData> occupancyData;

  HotelStatistics({
    required this.totalHotels,
    required this.activeHotels,
    required this.averageRating,
    required this.totalRooms,
    required this.availableRooms,
    required this.topHotels,
    required this.occupancyData,
  });

  factory HotelStatistics.fromJson(Map<String, dynamic> json) {
    return HotelStatistics(
      totalHotels: json['totalHotels'] ?? 0,
      activeHotels: json['activeHotels'] ?? 0,
      averageRating: json['averageRating']?.toDouble() ?? 0.0,
      totalRooms: json['totalRooms'] ?? 0,
      availableRooms: json['availableRooms'] ?? 0,
      topHotels: (json['topHotels'] as List?)
          ?.map((e) => TopHotelData.fromJson(e))
          .toList() ?? [],
      occupancyData: (json['occupancyData'] as List?)
          ?.map((e) => HotelOccupancyData.fromJson(e))
          .toList() ?? [],
    );
  }
}

class TopHotelData {
  final int hotelId;
  final String name;
  final double averageRating;
  final int totalBookings;
  final double totalRevenue;
  final double occupancyRate;

  TopHotelData({
    required this.hotelId,
    required this.name,
    required this.averageRating,
    required this.totalBookings,
    required this.totalRevenue,
    required this.occupancyRate,
  });

  factory TopHotelData.fromJson(Map<String, dynamic> json) {
    return TopHotelData(
      hotelId: json['hotelId'] ?? 0,
      name: json['name'] ?? '',
      averageRating: json['averageRating']?.toDouble() ?? 0.0,
      totalBookings: json['totalBookings'] ?? 0,
      totalRevenue: json['totalRevenue']?.toDouble() ?? 0.0,
      occupancyRate: json['occupancyRate']?.toDouble() ?? 0.0,
    );
  }
}

class HotelOccupancyData {
  final int hotelId;
  final String hotelName;
  final double occupancyRate;
  final int totalRooms;
  final int occupiedRooms;

  HotelOccupancyData({
    required this.hotelId,
    required this.hotelName,
    required this.occupancyRate,
    required this.totalRooms,
    required this.occupiedRooms,
  });

  factory HotelOccupancyData.fromJson(Map<String, dynamic> json) {
    return HotelOccupancyData(
      hotelId: json['hotelId'] ?? 0,
      hotelName: json['hotelName'] ?? '',
      occupancyRate: json['occupancyRate']?.toDouble() ?? 0.0,
      totalRooms: json['totalRooms'] ?? 0,
      occupiedRooms: json['occupiedRooms'] ?? 0,
    );
  }
}

class UserStatistics {
  final int totalUsers;
  final int activeUsers;
  final int newUsersThisMonth;
  final int newUsersThisYear;

  UserStatistics({
    required this.totalUsers,
    required this.activeUsers,
    required this.newUsersThisMonth,
    required this.newUsersThisYear,
  });

  factory UserStatistics.fromJson(Map<String, dynamic> json) {
    return UserStatistics(
      totalUsers: json['totalUsers'] ?? 0,
      activeUsers: json['activeUsers'] ?? 0,
      newUsersThisMonth: json['newUsersThisMonth'] ?? 0,
      newUsersThisYear: json['newUsersThisYear'] ?? 0,
    );
  }
}

class ReviewStatistics {
  final int totalReviews;
  final double averageRating;
  final int approvedReviews;
  final int pendingReviews;
  final int fiveStarReviews;
  final int fourStarReviews;
  final int threeStarReviews;
  final int twoStarReviews;
  final int oneStarReviews;

  ReviewStatistics({
    required this.totalReviews,
    required this.averageRating,
    required this.approvedReviews,
    required this.pendingReviews,
    required this.fiveStarReviews,
    required this.fourStarReviews,
    required this.threeStarReviews,
    required this.twoStarReviews,
    required this.oneStarReviews,
  });

  factory ReviewStatistics.fromJson(Map<String, dynamic> json) {
    return ReviewStatistics(
      totalReviews: json['totalReviews'] ?? 0,
      averageRating: json['averageRating']?.toDouble() ?? 0.0,
      approvedReviews: json['approvedReviews'] ?? 0,
      pendingReviews: json['pendingReviews'] ?? 0,
      fiveStarReviews: json['fiveStarReviews'] ?? 0,
      fourStarReviews: json['fourStarReviews'] ?? 0,
      threeStarReviews: json['threeStarReviews'] ?? 0,
      twoStarReviews: json['twoStarReviews'] ?? 0,
      oneStarReviews: json['oneStarReviews'] ?? 0,
    );
  }
}
