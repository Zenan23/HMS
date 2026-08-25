class InventoryItemCategory {
  final int id;
  final String name;

  InventoryItemCategory({required this.id, required this.name});

  factory InventoryItemCategory.fromJson(Map<String, dynamic> json) =>
      InventoryItemCategory(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
      );
}
