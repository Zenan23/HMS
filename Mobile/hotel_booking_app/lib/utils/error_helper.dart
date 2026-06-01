import 'package:flutter/material.dart';
import '../utils/api_response.dart';

void showApiError(BuildContext context, Object error) {
  final message = error is ApiException
      ? (error.statusCode == 403
          ? 'Nemate dozvolu za ovu akciju.'
          : error.message)
      : error.toString();
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(message),
      backgroundColor:
          error is ApiException && error.statusCode == 403 ? Colors.orange : Colors.red,
    ),
  );
}
