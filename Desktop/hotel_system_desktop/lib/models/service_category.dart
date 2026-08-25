class ServiceCategory {
  final int id;
  final String name;

  ServiceCategory({required this.id, required this.name});

  factory ServiceCategory.fromJson(Map<String, dynamic> json) =>
      ServiceCategory(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
      );
}
