enum UserRole {
  Guest,
  Employee,
  Admin,
}

UserRole userRoleFromInt(int value) => UserRole.values[value];
int userRoleToInt(UserRole role) => role.index;

class Employee {
  final int id;
  final String username;
  final String email;
  final String firstName;
  final String lastName;
  final String phoneNumber;
  final UserRole role;
  final bool isActive;
  final DateTime? lastLoginDate;
  final String fullName;
  final DateTime createdAt;
  final DateTime updatedAt;

  Employee({
    required this.id,
    required this.username,
    required this.email,
    required this.firstName,
    required this.lastName,
    required this.phoneNumber,
    required this.role,
    required this.isActive,
    required this.lastLoginDate,
    required this.fullName,
    required this.createdAt,
    required this.updatedAt,
  });

  factory Employee.fromJson(Map<String, dynamic> json) => Employee(
    id: json['id'],
    username: json['username'],
    email: json['email'],
    firstName: json['firstName'],
    lastName: json['lastName'],
    phoneNumber: json['phoneNumber'],
    role: userRoleFromInt(json['role']),
    isActive: json['isActive'],
    lastLoginDate: json['lastLoginDate'] != null ? DateTime.tryParse(json['lastLoginDate']) : null,
    fullName: json['fullName'] ?? '',
    createdAt: DateTime.parse(json['createdAt']),
    updatedAt: DateTime.parse(json['updatedAt']),
  );
}