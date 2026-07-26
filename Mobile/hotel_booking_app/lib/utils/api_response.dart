import 'dart:convert';
import 'package:http/http.dart' as http;

class ApiException implements Exception {
  final String message;
  final int statusCode;
  final List<String> errors;

  ApiException(this.message, {this.statusCode = 0, this.errors = const []});

  @override
  String toString() => message;
}

class PaginatedResult<T> {
  final List<T> items;
  final int totalCount;
  final int pageNumber;
  final int pageSize;

  PaginatedResult({
    required this.items,
    required this.totalCount,
    required this.pageNumber,
    required this.pageSize,
  });

  int get totalPages => pageSize > 0 ? (totalCount / pageSize).ceil() : 0;
}

class ApiResponseParser {
  static Map<String, dynamic> decodeBody(http.Response response) {
    if (response.body.isEmpty) return {};
    final decoded = jsonDecode(response.body);
    if (decoded is Map<String, dynamic>) return decoded;
    return {'data': decoded};
  }

  static String formatErrorMessage(String message, List<String> errors) {
    if (errors.isEmpty) return message;
    return '$message\n${errors.join('\n')}';
  }

  static void ensureSuccess(http.Response response) {
    if (response.statusCode == 403) {
      throw ApiException('Nemate dozvolu za ovu akciju.', statusCode: 403);
    }
    if (response.statusCode >= 400) {
      final decoded = decodeBody(response);
      final message = decoded['message'] ??
          decoded['Message'] ??
          'Greška: ${response.statusCode}';
      final errors = (decoded['errors'] as List?)
              ?.map((e) => e.toString())
              .toList() ??
          [];
      throw ApiException(formatErrorMessage(message.toString(), errors),
          statusCode: response.statusCode, errors: errors);
    }
  }

  static dynamic extractData(http.Response response) {
    ensureSuccess(response);
    final decoded = decodeBody(response);
    if (decoded.containsKey('success') && decoded['success'] == false) {
      throw ApiException(
        decoded['message']?.toString() ?? 'Operacija nije uspjela.',
        statusCode: response.statusCode,
      );
    }
    return decoded['data'];
  }

  static T parseObject<T>(
    http.Response response,
    T Function(Map<String, dynamic>) fromJson,
  ) {
    final data = extractData(response);
    if (data is Map<String, dynamic>) return fromJson(data);
    throw ApiException('Neočekivani format odgovora.');
  }

  static List<T> parseList<T>(
    http.Response response,
    T Function(Map<String, dynamic>) fromJson,
  ) {
    final data = extractData(response);
    if (data is List) {
      return data.map((e) => fromJson(e as Map<String, dynamic>)).toList();
    }
    throw ApiException('Neočekivani format liste.');
  }

  static PaginatedResult<T> parsePaginated<T>(
    http.Response response,
    T Function(Map<String, dynamic>) fromJson,
  ) {
    final data = extractData(response);
    if (data is Map<String, dynamic>) {
      final items = (data['items'] as List? ?? [])
          .map((e) => fromJson(e as Map<String, dynamic>))
          .toList();
      return PaginatedResult(
        items: items,
        totalCount: data['totalCount'] ?? items.length,
        pageNumber: data['pageNumber'] ?? 1,
        pageSize: data['pageSize'] ?? items.length,
      );
    }
    throw ApiException('Neočekivani paginirani format.');
  }
}
