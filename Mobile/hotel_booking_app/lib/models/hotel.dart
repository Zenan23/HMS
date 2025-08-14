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
  final double? averageRating;
  final int? reviewsCount;
  final DateTime createdAt;
  final DateTime updatedAt;
  final String imageUrl;

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
    this.averageRating,
    this.reviewsCount,
    required this.createdAt,
    required this.updatedAt,
    required this.imageUrl,
  });

  factory Hotel.fromJson(Map<String, dynamic> json) {
    return Hotel(
      id: json['id'],
      name: json['name'],
      address: json['address'],
      city: json['city'],
      country: json['country'],
      phoneNumber: json['phoneNumber'],
      email: json['email'],
      description: json['description'],
      starRating: json['starRating'],
      averageRating: (json['averageRating'] as num?)?.toDouble(),
      reviewsCount: json['reviewsCount'] as int?,
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt: DateTime.parse(json['updatedAt']),
      imageUrl: json['imageUrl'],
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt.toIso8601String(),
      'name': name,
      'address': address,
      'city': city,
      'country': country,
      'phoneNumber': phoneNumber,
      'email': email,
      'description': description,
      'starRating': starRating,
      'averageRating': averageRating,
      'reviewsCount': reviewsCount,
      'imageUrl': imageUrl,
    };
  }
}
