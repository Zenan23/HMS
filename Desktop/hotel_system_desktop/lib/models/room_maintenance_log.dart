class RoomMaintenanceLog {
  final int id;
  final int roomId;
  final String roomNumber;
  final DateTime reportedAt;
  final DateTime? resolvedAt;
  final String description;
  final double cost;
  final String technicianName;

  RoomMaintenanceLog({
    required this.id,
    required this.roomId,
    required this.reportedAt,
    this.resolvedAt,
    required this.description,
    required this.cost,
    required this.technicianName,
    this.roomNumber = '',
  });

  String get roomDisplayLabel =>
      roomNumber.isNotEmpty ? roomNumber : 'Soba #$roomId';

  factory RoomMaintenanceLog.fromJson(Map<String, dynamic> json) =>
      RoomMaintenanceLog(
        id: json['id'] ?? 0,
        roomId: json['roomId'] ?? 0,
        roomNumber: json['roomNumber'] ?? '',
        reportedAt:
            DateTime.tryParse(json['reportedAt'] ?? '') ?? DateTime.now(),
        resolvedAt: json['resolvedAt'] != null
            ? DateTime.tryParse(json['resolvedAt'])
            : null,
        description: json['description'] ?? '',
        cost: (json['cost'] ?? 0).toDouble(),
        technicianName: json['technicianName'] ?? '',
      );
}
