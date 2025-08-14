import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;
import 'api_service.dart';
import '../models/user.dart';

class AuthService extends ChangeNotifier {
  static const _storage = FlutterSecureStorage();
  static const _userKey = 'user';
  User? _user;

  User? get user => _user;
  String? get token => _user?.token;

  Future<String?> login(String email, String password) async {
    try {
      final response = await http.post(
        Uri.parse('${ApiService.baseUrl}/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'email': email, 'password': password}),
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as Map<String, dynamic>;
        // Spremi token i kompletne podatke o korisniku
        if (data['token'] is String) {
          await _storage.write(key: 'jwt_token', value: data['token']);
        }
        _user = User.fromJson(data);
        // Dozvoli login samo za Guest rolu (pretpostavka: role = 0 je Guest)
        if (_user!.role != 0) {
          _user = null;
          await _storage.delete(key: 'jwt_token');
          return 'Samo korisnici sa ulogom Guest mogu koristiti mobilnu aplikaciju.';
        }
        await _storage.write(key: _userKey, value: jsonEncode(_user!.toJson()));
        notifyListeners();
        return null;
      } else {
        return 'Neispravni podaci za prijavu';
      }
    } catch (e) {
      return 'Greška pri povezivanju sa serverom';
    }
  }

  Future<String?> register({
    required String username,
    required String email,
    required String password,
    required String confirmPassword,
    required String firstName,
    required String lastName,
    String? phoneNumber,
  }) async {
    try {
      final response = await http.post(
        Uri.parse('${ApiService.baseUrl}/auth/register'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'username': username,
          'email': email,
          'password': password,
          'confirmPassword': confirmPassword,
          'firstName': firstName,
          'lastName': lastName,
          'phoneNumber': phoneNumber ?? '',
        }),
      );
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body) as Map<String, dynamic>;
        if (data['token'] is String) {
          await _storage.write(key: 'jwt_token', value: data['token']);
        }
        _user = User.fromJson(data);
        if (_user!.role != 0) {
          _user = null;
          await _storage.delete(key: 'jwt_token');
          return 'Samo korisnici sa ulogom Guest mogu koristiti mobilnu aplikaciju.';
        }
        await _storage.write(key: _userKey, value: jsonEncode(_user!.toJson()));
        notifyListeners();
        return null;
      } else {
        return 'Greška pri registraciji';
      }
    } catch (e) {
      return 'Greška pri povezivanju sa serverom';
    }
  }

  Future<void> logout() async {
    _user = null;
    await _storage.delete(key: _userKey);
    notifyListeners();
  }

  void updateLocalUser(User updated) async {
    _user = updated;
    await _storage.write(key: _userKey, value: jsonEncode(updated.toJson()));
    notifyListeners();
  }

  Future<void> tryAutoLogin() async {
    final userJson = await _storage.read(key: _userKey);
    if (userJson != null) {
      _user = User.fromJson(jsonDecode(userJson));
      notifyListeners();
    }
  }
}
