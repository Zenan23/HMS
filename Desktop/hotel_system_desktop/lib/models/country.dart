class Country {
  final int id;
  final String name;

  Country({required this.id, required this.name});

  factory Country.fromJson(Map<String, dynamic> json) => Country(
        id: json['id'] ?? 0,
        name: json['name'] ?? '',
      );
}
