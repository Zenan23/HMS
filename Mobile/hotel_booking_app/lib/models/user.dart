class User {
  final int userId;
  final String token;
  final String email;
  final String username;
  final String firstName;
  final String lastName;
  final String phoneNumber;
  final int role; // usklađeno sa backend DTO (UserRole kao int)
  final DateTime expiresAt;

  User({
    required this.userId,
    required this.token,
    required this.email,
    required this.username,
    required this.firstName,
    required this.lastName,
    required this.phoneNumber,
    required this.role,
    required this.expiresAt,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    final dynamic roleRaw = json['role'];
    final int roleValue = roleRaw is int
        ? roleRaw
        : (roleRaw is String ? int.tryParse(roleRaw) ?? 0 : 0);
    return User(
      userId: json['userId'] ?? json['id'] ?? 0,
      token: json['token'] ?? '',
      email: json['email'] ?? '',
      username: json['username'] ?? '',
      firstName: json['firstName'] ?? '',
      lastName: json['lastName'] ?? '',
      phoneNumber: json['phoneNumber'] ?? '',
      role: roleValue,
      expiresAt: DateTime.tryParse(json['expiresAt'] ?? '') ?? DateTime.now().add(const Duration(hours: 1)),
    );
  }

  Map<String, dynamic> toJson() => {
        'userId': userId,
        'token': token,
        'email': email,
        'username': username,
        'firstName': firstName,
        'lastName': lastName,
        'phoneNumber': phoneNumber,
        'role': role,
        'expiresAt': expiresAt.toIso8601String(),
      };
}
