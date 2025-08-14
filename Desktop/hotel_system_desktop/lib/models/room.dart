enum RoomType { Single, Double, Twin, Suite, Deluxe, Presidential }

RoomType roomTypeFromInt(int value) {
  final idx = (value - 1).clamp(0, RoomType.values.length - 1);
  return RoomType.values[idx];
}
int roomTypeToInt(RoomType role) => role.index + 1;

class Room {
  final int id;
  final String roomNumber;
  final RoomType roomType;
  final double pricePerNight;
  final int maxOccupancy;
  final String description;
  final bool isAvailable;
  final int hotelId;
  final String? hotelName;
  final DateTime createdAt;
  final DateTime updatedAt;

  Room({
    required this.id,
    required this.roomNumber,
    required this.roomType,
    required this.pricePerNight,
    required this.maxOccupancy,
    required this.description,
    required this.isAvailable,
    required this.hotelId,
    required this.createdAt,
    required this.updatedAt,
    this.hotelName,
  });

  factory Room.fromJson(Map<String, dynamic> json) => Room(
        id: json['id'],
        roomNumber: json['roomNumber'],
        roomType: roomTypeFromInt(json['roomType'] ?? 1),
        pricePerNight: (json['pricePerNight'] as num).toDouble(),
        maxOccupancy: json['maxOccupancy'],
        description: json['description'] ?? '',
        isAvailable: json['isAvailable'] ?? false,
        hotelId: json['hotelId'],
        hotelName: json['hotelName'],
        createdAt: DateTime.parse(json['createdAt']),
        updatedAt: DateTime.parse(json['updatedAt']),
      );
}
