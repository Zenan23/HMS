class City {
  final int id;
  final String name;
  final int countryId;
  final String countryName;

  City({
    required this.id,
    required this.name,
    required this.countryId,
    required this.countryName,
  });

  String get label => countryName.isNotEmpty ? '$name, $countryName' : name;

  factory City.fromJson(Map<String, dynamic> json) => City(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
        countryId: json['countryId'] ?? 0,
        countryName: json['countryName'] ?? '',
      );
}
