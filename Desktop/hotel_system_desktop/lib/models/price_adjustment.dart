class PriceAdjustment {
  final int id;
  final String name;
  final double percentageModifier;
  final DateTime startDate;
  final DateTime endDate;
  final bool isCumulative;
  final int? hotelId;
  final String hotelName;
  final String createdByUserName;

  PriceAdjustment({
    required this.id,
    required this.name,
    required this.percentageModifier,
    required this.startDate,
    required this.endDate,
    required this.isCumulative,
    required this.hotelId,
    required this.hotelName,
    required this.createdByUserName,
  });

  factory PriceAdjustment.fromJson(Map<String, dynamic> json) => PriceAdjustment(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
        percentageModifier: (json['percentageModifier'] ?? 0).toDouble(),
        startDate: DateTime.tryParse(json['startDate'] ?? '') ?? DateTime.now(),
        endDate: DateTime.tryParse(json['endDate'] ?? '') ?? DateTime.now(),
        isCumulative: json['isCumulative'] ?? false,
        hotelId: json['hotelId'],
        hotelName: json['hotelName'] ?? '',
        createdByUserName: json['createdByUserName'] ?? '',
      );
}
