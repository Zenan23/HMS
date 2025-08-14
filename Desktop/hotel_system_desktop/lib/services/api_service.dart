import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class ApiService {
  static const String baseUrl = 'http://localhost:8080';
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  Future<String?> getToken() async => await _storage.read(key: 'jwt_token');

  Future<http.Response> get(String endpoint, {bool auth = true}) async {
    final headers = <String, String>{'Content-Type': 'application/json'};
    if (auth) {
      final token = await getToken();
      if (token != null) headers['Authorization'] = 'Bearer $token';
    }
    final response = await http.get(Uri.parse('$baseUrl$endpoint'), headers: headers);
    _handleError(response);
    return response;
  }

  Future<http.Response> post(String endpoint, Map<String, dynamic> body, {bool auth = true}) async {
    final headers = <String, String>{'Content-Type': 'application/json'};
    if (auth) {
      final token = await getToken();
      if (token != null) headers['Authorization'] = 'Bearer $token';
    }
    final response = await http.post(Uri.parse('$baseUrl$endpoint'), headers: headers, body: jsonEncode(body));
    _handleError(response);
    return response;
  }

  Future<http.Response> put(String endpoint, Map<String, dynamic> body, {bool auth = true}) async {
    final headers = <String, String>{'Content-Type': 'application/json'};
    if (auth) {
      final token = await getToken();
      if (token != null) headers['Authorization'] = 'Bearer $token';
    }
    final response = await http.put(Uri.parse('$baseUrl$endpoint'), headers: headers, body: jsonEncode(body));
    _handleError(response);
    return response;
  }

  Future<http.Response> delete(String endpoint, {bool auth = true}) async {
    final headers = <String, String>{'Content-Type': 'application/json'};
    if (auth) {
      final token = await getToken();
      if (token != null) headers['Authorization'] = 'Bearer $token';
    }
    final response = await http.delete(Uri.parse('$baseUrl$endpoint'), headers: headers);
    _handleError(response);
    return response;
  }

  void _handleError(http.Response response) {
    if (response.statusCode >= 400) {
      throw Exception('Greška:  ${response.statusCode} - ${response.body}');
    }
  }
}