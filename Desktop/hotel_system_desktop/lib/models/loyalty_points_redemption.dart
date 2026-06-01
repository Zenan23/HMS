class LoyaltyPointsRedemption {
  final int id;
  final int userId;
  final String userName;
  final int bookingId;
  final int pointsUsed;
  final DateTime redeemedAt;
  final double equivalentValueAmount;

  LoyaltyPointsRedemption({
    required this.id,
    required this.userId,
    required this.userName,
    required this.bookingId,
    required this.pointsUsed,
    required this.redeemedAt,
    required this.equivalentValueAmount,
  });

  factory LoyaltyPointsRedemption.fromJson(Map<String, dynamic> json) =>
      LoyaltyPointsRedemption(
        id: json['id'] ?? 0,
        userId: json['userId'] ?? 0,
        userName: json['userName'] ?? '',
        bookingId: json['bookingId'] ?? 0,
        pointsUsed: json['pointsUsed'] ?? 0,
        redeemedAt:
            DateTime.tryParse(json['redeemedAt'] ?? '') ?? DateTime.now(),
        equivalentValueAmount: (json['equivalentValueAmount'] ?? 0).toDouble(),
      );
}
