import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:flutter/foundation.dart';

class ApiService {
  static const _storage = FlutterSecureStorage();
  static final String baseUrl = () {
    if (kIsWeb) {
      final host = Uri.base.host;
      // Lokalno web dev okruženje
      if (host == 'localhost' || host == '127.0.0.1') {
        return 'http://localhost:8080/api';
      }
      // Docker okruženje: API servis je 'api' na 8080
      return 'http://localhost:8080/api';
    }
    if (defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:8080/api';
    }
    // iOS simulator/desktop
    return 'http://localhost:8080/api';
  }();

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
}
