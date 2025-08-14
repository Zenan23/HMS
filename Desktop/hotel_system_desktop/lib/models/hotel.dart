class Hotel {
  final int id;
  final String name;
  final String address;
  final String city;
  final String country;
  final String phoneNumber;
  final String email;
  final String description;
  final int starRating;
  final String imageUrl;
  final double averageRating;
  final DateTime createdAt;
  final DateTime updatedAt;

  Hotel({
    required this.id,
    required this.name,
    required this.address,
    required this.city,
    required this.country,
    required this.phoneNumber,
    required this.email,
    required this.description,
    required this.starRating,
    required this.imageUrl,
    required this.createdAt,
    required this.updatedAt,
    required this.averageRating,
  });

  factory Hotel.fromJson(Map<String, dynamic> json) => Hotel(
        id: json['id'],
        name: json['name'],
        address: json['address'],
        city: json['city'],
        country: json['country'],
        phoneNumber: json['phoneNumber'],
        email: json['email'],
        description: json['description'],
        starRating: json['starRating'],
        imageUrl: json['imageUrl'] ?? '',
        createdAt: DateTime.parse(json['createdAt']),
        updatedAt: DateTime.parse(json['updatedAt']),
        averageRating: json['averageRating'].toDouble(),
      );
}
