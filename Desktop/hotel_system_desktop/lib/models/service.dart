class Service {
  final int id;
  final String name;
  final String description;
  final double price;
  final String category;
  final int serviceCategoryId;
  final bool isAvailable;
  final bool isActive;
  final int hotelId;
  final String? hotelName;
  final DateTime createdAt;
  final DateTime updatedAt;

  Service({
    required this.id,
    required this.name,
    required this.description,
    required this.price,
    required this.category,
    required this.serviceCategoryId,
    required this.isAvailable,
    required this.isActive,
    required this.hotelId,
    this.hotelName,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Service.fromJson(Map<String, dynamic> json) => Service(
        id: json['id'],
        name: json['name'],
        description: json['description'] ?? '',
        price: (json['price'] as num).toDouble(),
        category: json['category'] ?? '',
        serviceCategoryId: json['serviceCategoryId'] ?? 0,
        isAvailable: json['isAvailable'] ?? true,
        isActive: json['isActive'] ?? true,
        hotelId: json['hotelId'],
        hotelName: json['hotelName'],
        createdAt: DateTime.parse(json['createdAt']),
        updatedAt: DateTime.parse(json['updatedAt']),
      );
}
