import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../config/app_config.dart';

class ApiService {
  static const _storage = FlutterSecureStorage();
  static const String baseUrl = AppConfig.baseUrl;

  /// Korijen API servera bez /api — za static uploads (/uploads/...).
  static String get apiOrigin => Uri.parse(baseUrl).origin;

  static Future<String?> getToken() async {
    // Primarni ključ: usklađeno sa desktop klijentom
    final directToken = await _storage.read(key: 'jwt_token');
    if (directToken != null && directToken.isNotEmpty) return directToken;

    // Fallback: pokušaj iz spremljenog korisnika
    final userJson = await _storage.read(key: 'user');
    if (userJson == null) return null;
    try {
      final Map<String, dynamic> data =
          jsonDecode(userJson) as Map<String, dynamic>;
      final token = data['token'];
      if (token is String && token.isNotEmpty) return token;
      return null;
    } catch (_) {
      return null;
    }
  }

  static Future<http.Response> get(String endpoint) async {
    final token = await getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
    final url = Uri.parse('$baseUrl$endpoint');
    return http.get(url, headers: headers);
  }

  static Future<http.Response> post(
      String endpoint, Map<String, dynamic> body) async {
    final token = await getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
    final url = Uri.parse('$baseUrl$endpoint');
    return http.post(url, headers: headers, body: jsonEncode(body));
  }

  static Future<http.Response> patch(
      String endpoint, Map<String, dynamic> body) async {
    final token = await getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
    final url = Uri.parse('$baseUrl$endpoint');
    return http.patch(url, headers: headers, body: jsonEncode(body));
  }


  static Future<http.Response> put(
      String endpoint, Map<String, dynamic> body) async {
    final token = await getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
    final url = Uri.parse('$baseUrl$endpoint');
    return http.put(url, headers: headers, body: jsonEncode(body));
  }

  static Future<http.Response> delete(String endpoint) async {
    final token = await getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
    final url = Uri.parse('$baseUrl$endpoint');
    return http.delete(url, headers: headers);
  }
}
