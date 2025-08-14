import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class AuthService {
  static const _storage = FlutterSecureStorage();
  static const _tokenKey = 'jwt_token';
  static const _userIdKey = 'user_id';
  static const _emailKey = 'email';
  static const _usernameKey = 'username';
  static const _firstNameKey = 'first_name';
  static const _lastNameKey = 'last_name';
  static const _roleKey = 'role';
  static const _expiresAtKey = 'expires_at';

  Future<void> login(String email, String password) async {
    final response = await http.post(
      Uri.parse('http://localhost:8080/api/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      await _storage.write(key: _tokenKey, value: data['token']);
      await _storage.write(key: _userIdKey, value: data['userId'].toString());
      await _storage.write(key: _emailKey, value: data['email']);
      await _storage.write(key: _usernameKey, value: data['username']);
      await _storage.write(key: _firstNameKey, value: data['firstName']);
      await _storage.write(key: _lastNameKey, value: data['lastName']);
      await _storage.write(key: _roleKey, value: data['role']?.toString());
      await _storage.write(key: _expiresAtKey, value: data['expiresAt']);
    } else {
      throw Exception('Neispravni podaci za prijavu');
    }
  }

  Future<void> logout() async {
    await _storage.deleteAll();
  }

  Future<bool> hasToken() async {
    final token = await _storage.read(key: _tokenKey);
    return token != null;
  }

  // Dodatne metode za dohvat korisničkih podataka po potrebi
  Future<String?> getToken() async => await _storage.read(key: _tokenKey);
  Future<String?> getUserId() async => await _storage.read(key: _userIdKey);
  Future<String?> getEmail() async => await _storage.read(key: _emailKey);
  Future<String?> getUsername() async => await _storage.read(key: _usernameKey);
  Future<String?> getFirstName() async => await _storage.read(key: _firstNameKey);
  Future<String?> getLastName() async => await _storage.read(key: _lastNameKey);
  Future<String?> getRole() async => await _storage.read(key: _roleKey);
  Future<int?> getRoleInt() async {
    final raw = await _storage.read(key: _roleKey);
    if (raw == null) return null;
    return int.tryParse(raw);
  }
  Future<String?> getExpiresAt() async => await _storage.read(key: _expiresAtKey);
}