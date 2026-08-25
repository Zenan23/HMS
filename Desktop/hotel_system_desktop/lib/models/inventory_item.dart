class InventoryItem {
  final int id;
  final String name;
  final String unit;
  final String category;
  final int inventoryItemCategoryId;
  final int minimumStockLevel;

  InventoryItem({
    required this.id,
    required this.name,
    required this.unit,
    required this.category,
    required this.inventoryItemCategoryId,
    required this.minimumStockLevel,
  });

  factory InventoryItem.fromJson(Map<String, dynamic> json) => InventoryItem(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
        unit: json['unit'] ?? '',
        category: json['category'] ?? '',
        inventoryItemCategoryId: json['inventoryItemCategoryId'] ?? 0,
        minimumStockLevel: json['minimumStockLevel'] ?? 0,
      );
}
