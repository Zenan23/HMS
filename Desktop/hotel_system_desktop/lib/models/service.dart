class Service {
  final int id;
  final String name;
  final String description;
  final double price;
  final String category;
  final bool isAvailable;
  final bool isActive;
  final int hotelId;
  final DateTime createdAt;
  final DateTime updatedAt;

  Service({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.category,
    required this.isAvailable,
    required this.isActive,
    required this.hotelId,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Service.fromJson(Map<String, dynamic> json) => Service(
        id: json['id'],
        name: json['name'],
        description: json['description'] ?? '',
        price: (json['price'] as num).toDouble(),
        category: json['category'] ?? '',
        isAvailable: json['isAvailable'] ?? true,
        isActive: json['isActive'] ?? true,
        hotelId: json['hotelId'],
        createdAt: DateTime.parse(json['createdAt']),
        updatedAt: DateTime.parse(json['updatedAt']),
      );
}