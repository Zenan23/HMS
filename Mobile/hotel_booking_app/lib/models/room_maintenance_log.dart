class RoomMaintenanceLog {
  final int id;
  final int roomId;
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
  });

  factory RoomMaintenanceLog.fromJson(Map<String, dynamic> json) =>
      RoomMaintenanceLog(
        id: json['id'] ?? 0,
        roomId: json['roomId'] ?? 0,
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
