enum RoomType { Single, Double, Twin, Suite, Deluxe, Presidential }

class Room {
  final int id;
  final DateTime createdAt;
  final DateTime updatedAt;
  final String roomNumber;
  final RoomType roomType;
  final double pricePerNight;
  final int maxOccupancy;
  final String description;
  final bool isAvailable;
  final int hotelId;
  final String hotelName;

  Room({
    required this.id,
    required this.createdAt,
    required this.updatedAt,
    required this.roomNumber,
    required this.roomType,
    required this.pricePerNight,
    required this.maxOccupancy,
    required this.description,
    required this.isAvailable,
    required this.hotelId,
    required this.hotelName,
  });

  factory Room.fromJson(Map<String, dynamic> json) {
    return Room(
      id: json['id'],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: DateTime.parse(json['updatedAt']),
      roomNumber: json['roomNumber'] ?? '',
      roomType: _parseRoomType(json['roomType']),
      pricePerNight: (json['pricePerNight'] as num).toDouble(),
      maxOccupancy: json['maxOccupancy'] ?? 1,
      description: json['description'] ?? '',
      isAvailable: json['isAvailable'] ?? true,
      hotelId: json['hotelId'],
      hotelName: json['hotelName'] ?? '',
    );
  }

  static RoomType _parseRoomType(dynamic roomType) {
    if (roomType is int) {
      switch (roomType) {
        case 1: return RoomType.Single;
        case 2: return RoomType.Double;
        case 3: return RoomType.Twin;
        case 4: return RoomType.Suite;
        case 5: return RoomType.Deluxe;
        case 6: return RoomType.Presidential;
        default: return RoomType.Single;
      }
    }
    return RoomType.Single;
  }

  String get roomTypeString {
    switch (roomType) {
      case RoomType.Single: return 'Single';
      case RoomType.Double: return 'Double';
      case RoomType.Twin: return 'Twin';
      case RoomType.Suite: return 'Suite';
      case RoomType.Deluxe: return 'Deluxe';
      case RoomType.Presidential: return 'Presidential';
    }
  }

  int roomTypeToInt(RoomType type) {
    switch (type) {
      case RoomType.Single: return 1;
      case RoomType.Double: return 2;
      case RoomType.Twin: return 3;
      case RoomType.Suite: return 4;
      case RoomType.Deluxe: return 5;
      case RoomType.Presidential: return 6;
    }
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
      'roomNumber': roomNumber,
      'roomType': roomTypeToInt(roomType),
      'pricePerNight': pricePerNight,
      'maxOccupancy': maxOccupancy,
      'description': description,
      'isAvailable': isAvailable,
      'hotelId': hotelId,
      'hotelName': hotelName,
    };
  }
}