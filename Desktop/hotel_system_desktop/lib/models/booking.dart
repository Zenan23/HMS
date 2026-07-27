enum BookingStatus {
  Pending,
  Confirmed,
  CheckedIn,
  CheckedOut,
  Cancelled,
  NoShow
}

// Backend enum je 1-based (Pending=1,...), dok je Dart enum 0-based.
// Mapiramo sigurno uz granice.
BookingStatus bookingStatusFromInt(int value) {
  final idx = (value - 1).clamp(0, BookingStatus.values.length - 1);
  return BookingStatus.values[idx];
}

int bookingStatusToInt(BookingStatus status) => status.index + 1;

class Booking {
  final int id;
  final DateTime checkInDate;
  final DateTime checkOutDate;
  final int numberOfGuests;
  final double totalPrice;
  final BookingStatus status;
  final String specialRequests;
  final int roomId;
  final String roomNumber;
  final String? hotelName;
  final int userId;
  final String userName;
  final DateTime createdAt;
  final DateTime updatedAt;
  final List<BookingServiceItem> services;

  Booking({
    required this.id,
    required this.checkInDate,
    required this.checkOutDate,
    required this.numberOfGuests,
    required this.totalPrice,
    required this.status,
    required this.specialRequests,
    required this.roomId,
    required this.userId,
    required this.createdAt,
    required this.updatedAt,
    required this.services,
    this.roomNumber = '',
    this.hotelName,
    this.userName = '',
  });

  String get roomDisplayLabel {
    if (roomNumber.isEmpty) return 'Soba #$roomId';
    if (hotelName != null && hotelName!.isNotEmpty) {
      return '$roomNumber ($hotelName)';
    }
    return roomNumber;
  }

  String get userDisplayLabel =>
      userName.isNotEmpty ? userName : 'Korisnik #$userId';

  factory Booking.fromJson(Map<String, dynamic> json) => Booking(
        id: json['id'] ?? 0,
        checkInDate:
            DateTime.tryParse(json['checkInDate'] ?? '') ?? DateTime(1900),
        checkOutDate:
            DateTime.tryParse(json['checkOutDate'] ?? '') ?? DateTime(1900),
        numberOfGuests: json['numberOfGuests'] ?? 0,
        totalPrice: (json['totalPrice'] ?? 0).toDouble(),
        status: bookingStatusFromInt(json['status'] ?? 0),
        specialRequests: json['specialRequests'] ?? '',
        roomId: json['roomId'] ?? 0,
        roomNumber: json['roomNumber'] ?? '',
        hotelName: json['hotelName'],
        userId: json['userId'] ?? 0,
        userName: json['userName'] ?? '',
        createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime(1900),
        updatedAt: DateTime.tryParse(json['updatedAt'] ?? '') ?? DateTime(1900),
        services: ((json['services'] ?? []) as List)
            .map((e) => BookingServiceItem.fromJson(e))
            .toList(),
      );
}

class BookingServiceItem {
  final int serviceId;
  final String? serviceName;
  final double unitPrice;
  final int quantity;

  BookingServiceItem({
    required this.serviceId,
    required this.unitPrice,
    required this.quantity,
    this.serviceName,
  });

  factory BookingServiceItem.fromJson(Map<String, dynamic> json) => BookingServiceItem(
        serviceId: json['serviceId'] ?? 0,
        unitPrice: (json['unitPrice'] ?? 0).toDouble(),
        quantity: json['quantity'] ?? 1,
        serviceName: json['service']?['name'] ?? json['serviceName'],
      );
}
