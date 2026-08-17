import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../config/app_config.dart';
import '../main.dart' show navigatorKey;

class ApiService {
  static const _storage = FlutterSecureStorage();
  static const String baseUrl = AppConfig.baseUrl;

  // Sprječava da više paralelnih 401 odgovora (npr. nekoliko istovremenih poziva nakon isteka
  // tokena) pokrene redirect na login više puta zaredom.
  static bool _handlingUnauthorized = false;

  /// Centralizovana obrada HTTP 401 odgovora — token je istekao ili je poništen (logout na
  /// drugom uređaju, server-side revoke i sl.). Briše lokalno sačuvane podatke i vraća korisnika
  /// na login ekran. Uputa (Dodatak A.2): "Frontend mora pravilno obraditi HTTP 401 odgovor
  /// (redirect na login ili refresh token mehanizam)."
  static Future<void> _handleUnauthorized() async {
    if (_handlingUnauthorized) return;
    _handlingUnauthorized = true;
    try {
      await _storage.delete(key: 'jwt_token');
      await _storage.delete(key: 'user');

      final navState = navigatorKey.currentState;
      if (navState != null) {
        navState.pushNamedAndRemoveUntil('/login', (route) => false);
        final ctx = navState.context;
        WidgetsBinding.instance.addPostFrameCallback((_) {
          ScaffoldMessenger.of(ctx).showSnackBar(
            const SnackBar(
              content: Text('Sesija je istekla. Prijavite se ponovo.'),
              backgroundColor: Colors.orange,
            ),
          );
        });
      }
    } finally {
      _handlingUnauthorized = false;
    }
  }

  static Future<http.Response> _withUnauthorizedCheck(
      Future<http.Response> Function() request) async {
    final response = await request();
    if (response.statusCode == 401) {
      // Ne čekaj (fire-and-forget) — pozivalac i dalje dobija originalni 401 response da
      // odgovarajući ekran može prikazati grešku ako je uhvati prije nego se redirect desi.
      unawaited(_handleUnauthorized());
    }
    return response;
  }

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
    return _withUnauthorizedCheck(() async {
      final token = await getToken();
      final headers = {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };
      final url = Uri.parse('$baseUrl$endpoint');
      return http.get(url, headers: headers);
    });
  }

  static Future<http.Response> post(
      String endpoint, Map<String, dynamic> body) async {
    return _withUnauthorizedCheck(() async {
      final token = await getToken();
      final headers = {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };
      final url = Uri.parse('$baseUrl$endpoint');
      return http.post(url, headers: headers, body: jsonEncode(body));
    });
  }

  static Future<http.Response> patch(
      String endpoint, Map<String, dynamic> body) async {
    return _withUnauthorizedCheck(() async {
      final token = await getToken();
      final headers = {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };
      final url = Uri.parse('$baseUrl$endpoint');
      return http.patch(url, headers: headers, body: jsonEncode(body));
    });
  }

  static Future<http.Response> put(
      String endpoint, Map<String, dynamic> body) async {
    return _withUnauthorizedCheck(() async {
      final token = await getToken();
      final headers = {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };
      final url = Uri.parse('$baseUrl$endpoint');
      return http.put(url, headers: headers, body: jsonEncode(body));
    });
  }

  static Future<http.Response> delete(String endpoint) {
    return _withUnauthorizedCheck(() async {
      final token = await getToken();
      final headers = {
        'Content-Type': 'application/json',
        if (token != null) 'Authorization': 'Bearer $token',
      };
      final url = Uri.parse('$baseUrl$endpoint');
      return http.delete(url, headers: headers);
    });
  }
}
