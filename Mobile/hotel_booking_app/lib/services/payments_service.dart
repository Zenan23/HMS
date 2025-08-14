import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/payment.dart';
import 'api_service.dart';

class PaymentsService {
  Future<bool> processPayment(CreatePaymentDto dto) async {
    final payload = dto.toJson();
    print('Payment payload: ' + jsonEncode(payload));
    final response = await ApiService.post('/Payments/process', payload);
    if (response.statusCode == 200 || response.statusCode == 201) {
      return true;
    } else {
      return false;
    }
  }
}