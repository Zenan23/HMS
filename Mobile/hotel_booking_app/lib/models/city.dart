class City {
  final int id;
  final String name;
  final String countryName;

  City({required this.id, required this.name, required this.countryName});

  String get label => countryName.isNotEmpty ? '$name, $countryName' : name;

  factory City.fromJson(Map<String, dynamic> json) => City(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
        countryName: json['countryName'] ?? '',
      );
}
