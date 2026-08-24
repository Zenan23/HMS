class ReservationServiceItem {
  final int serviceId;
  final int quantity;
  final double unitPrice;
  final String? serviceName;

  ReservationServiceItem({
    required this.serviceId,
    required this.quantity,
    required this.unitPrice,
    this.serviceName,
  });

  factory ReservationServiceItem.fromJson(Map<String, dynamic> json) {
    return ReservationServiceItem(
      serviceId: json['serviceId'] ?? 0,
      quantity: json['quantity'] ?? 1,
      unitPrice: (json['unitPrice'] as num?)?.toDouble() ?? 0,
      serviceName: json['serviceName']?.toString(),
    );
  }

  double get lineTotal => unitPrice * quantity;
}

class Reservation {
  final int id;
  final DateTime? checkInDate;
  final DateTime? checkOutDate;
  final int numberOfGuests;
  final num totalPrice;
  final int status;
  final String specialRequests;
  final int roomId;
  final int userId;
  final DateTime? createdAt;
  final DateTime? updatedAt;
  final List<ReservationServiceItem> services;

  Reservation({
    required this.id,
    this.checkInDate,
    this.checkOutDate,
    required this.numberOfGuests,
    required this.totalPrice,
    required this.status,
    required this.specialRequests,
    required this.roomId,
    required this.userId,
    this.createdAt,
    this.updatedAt,
    this.services = const [],
  });

  String get statusLabel {
    switch (status) {
      case 1:
        return 'Na čekanju';
      case 2:
        return 'Potvrđena';
      case 3:
        return 'Check-in';
      case 4:
        return 'Check-out';
      case 5:
        return 'Otkazana';
      case 6:
        return 'Nedolazak';
      default:
        return 'Nepoznato';
    }
  }

  factory Reservation.fromJson(Map<String, dynamic> json) {
    final servicesJson = json['services'] as List?;
    return Reservation(
      id: json['id'] ?? 0,
      checkInDate: json['checkInDate'] != null
          ? DateTime.tryParse(json['checkInDate'].toString())
          : null,
      checkOutDate: json['checkOutDate'] != null
          ? DateTime.tryParse(json['checkOutDate'].toString())
          : null,
      numberOfGuests: json['numberOfGuests'] ?? 0,
      totalPrice: json['totalPrice'] ?? 0,
      status: json['status'] ?? 0,
      specialRequests: json['specialRequests']?.toString() ?? '',
      roomId: json['roomId'] ?? 0,
      userId: json['userId'] ?? json['guestId'] ?? 0,
      createdAt: json['createdAt'] != null
          ? DateTime.tryParse(json['createdAt'].toString())
          : null,
      updatedAt: json['updatedAt'] != null
          ? DateTime.tryParse(json['updatedAt'].toString())
          : null,
      services: servicesJson != null
          ? servicesJson
              .map((e) =>
                  ReservationServiceItem.fromJson(e as Map<String, dynamic>))
              .toList()
          : [],
    );
  }
}
