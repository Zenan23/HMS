class InventoryTransaction {
  final int id;
  final int inventoryItemId;
  final int quantityChange;
  final DateTime transactionDate;
  final int staffUserId;
  final String staffUserName;
  final String reason;

  InventoryTransaction({
    required this.id,
    required this.inventoryItemId,
    required this.quantityChange,
    required this.transactionDate,
    required this.staffUserId,
    required this.staffUserName,
    required this.reason,
  });

  factory InventoryTransaction.fromJson(Map<String, dynamic> json) =>
      InventoryTransaction(
        id: json['id'] ?? 0,
        inventoryItemId: json['inventoryItemId'] ?? 0,
        quantityChange: json['quantityChange'] ?? 0,
        transactionDate:
            DateTime.tryParse(json['transactionDate'] ?? '') ?? DateTime.now(),
        staffUserId: json['staffUserId'] ?? 0,
        staffUserName: json['staffUserName'] ?? '',
        reason: json['reason'] ?? '',
      );
}
