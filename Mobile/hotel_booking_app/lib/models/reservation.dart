class Reservation {
  final int id;
  final DateTime? checkInDate;
  final DateTime? checkOutDate;
  final int numberOfGuests;
  final num totalPrice;
  final int status;
  final String specialRequests;
  final int roomId;
  final int guestId;
  final int userId;
  final DateTime? createdAt;
  final DateTime? updatedAt;

  Reservation({
    required this.id,
    this.checkInDate,
    this.checkOutDate,
    required this.numberOfGuests,
    required this.totalPrice,
    required this.status,
    required this.specialRequests,
    required this.roomId,
    required this.guestId,
    required this.userId,
    this.createdAt,
    this.updatedAt,
  });

  factory Reservation.fromJson(Map<String, dynamic> json) {
    return Reservation(
      id: json['id'] ?? 0,
      checkInDate: json['checkInDate'] != null ? DateTime.tryParse(json['checkInDate']) : null,
      checkOutDate: json['checkOutDate'] != null ? DateTime.tryParse(json['checkOutDate']) : null,
      numberOfGuests: json['numberOfGuests'] ?? 0,
      totalPrice: json['totalPrice'] ?? 0,
      status: json['status'] ?? 0,
      specialRequests: json['specialRequests'] ?? '',
      roomId: json['roomId'] ?? 0,
      guestId: json['guestId'] ?? 0,
      userId: json['userId'] ?? 0,
      createdAt: json['createdAt'] != null ? DateTime.tryParse(json['createdAt']) : null,
      updatedAt: json['updatedAt'] != null ? DateTime.tryParse(json['updatedAt']) : null,
    );
  }
}