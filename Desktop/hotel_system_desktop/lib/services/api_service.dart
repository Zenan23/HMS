import 'dart:convert';
import 'package:file_picker/file_picker.dart';
import 'package:http/http.dart' as http;
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../config/app_config.dart';
import '../models/dashboard_statistics.dart';
import '../utils/api_response.dart';

class ApiService {
  static const String baseUrl = AppConfig.baseUrl;
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

  Future<void> uploadHotelImage(int hotelId, PlatformFile file) async {
    final uri = Uri.parse('$baseUrl/api/hotels/$hotelId/image');
    final request = http.MultipartRequest('POST', uri);
    final token = await getToken();
    if (token != null) request.headers['Authorization'] = 'Bearer $token';

    if (file.bytes != null) {
      request.files.add(http.MultipartFile.fromBytes(
        'file',
        file.bytes!,
        filename: file.name,
      ));
    } else if (file.path != null) {
      request.files.add(await http.MultipartFile.fromPath('file', file.path!, filename: file.name));
    } else {
      throw ApiException('Odabrana slika nije dostupna.');
    }

    final streamed = await request.send();
    final response = await http.Response.fromStream(streamed);
    _handleError(response);
  }

  void _handleError(http.Response response) {
    ApiResponseParser.ensureSuccess(response);
  }

  Future<DashboardStatistics> getDashboardStatistics({
    DateTime? fromDate,
    DateTime? toDate,
  }) async {
    final queryParams = <String, String>{};
    if (fromDate != null) {
      queryParams['fromDate'] = fromDate.toIso8601String();
    }
    if (toDate != null) {
      queryParams['toDate'] = toDate.toIso8601String();
    }

    final uri = Uri.parse('$baseUrl/api/Dashboard/statistics').replace(queryParameters: queryParams);
    final headers = <String, String>{'Content-Type': 'application/json'};
    
    final token = await getToken();
    if (token != null) headers['Authorization'] = 'Bearer $token';

    final response = await http.get(uri, headers: headers);
    _handleError(response);

    final jsonResponse = jsonDecode(response.body);
    if (jsonResponse['success'] == true) {
      return DashboardStatistics.fromJson(jsonResponse['data']);
    } else {
      throw Exception(jsonResponse['message'] ?? 'Greška pri učitavanju statistike');
    }
  }
}