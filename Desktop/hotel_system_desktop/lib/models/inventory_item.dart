class InventoryItem {
  final int id;
  final String name;
  final String unit;
  final String category;
  final int minimumStockLevel;

  InventoryItem({
    required this.id,
    required this.name,
    required this.unit,
    required this.category,
    required this.minimumStockLevel,
  });

  factory InventoryItem.fromJson(Map<String, dynamic> json) => InventoryItem(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
        unit: json['unit'] ?? '',
        category: json['category'] ?? '',
        minimumStockLevel: json['minimumStockLevel'] ?? 0,
      );
}
