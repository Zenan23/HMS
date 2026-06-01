import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/payment.dart';
import 'api_service.dart';

class PaymentsService {
  /// Vraća [HostedCheckoutResponse] ili null ako API nije uspio.
  Future<HostedCheckoutResponse?> startHostedCheckout(CreateHostedCheckoutDto dto) async {
    final response = await ApiService.post('/Payments/hosted-checkout', dto.toJson());
    if (response.statusCode != 200 && response.statusCode != 201) {
      return null;
    }
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      final data = map['data'] as Map<String, dynamic>?;
      if (data == null) return null;
      return HostedCheckoutResponse.fromJson(data);
    } catch (_) {
      return null;
    }
  }

  /// Provjera statusa plaćanja (polling). Vraća true ako je completed (3).
  Future<bool> isPaymentCompleted(int paymentId) async {
    final response = await ApiService.get('/Payments/$paymentId');
    if (response.statusCode != 200) return false;
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      final data = map['data'] as Map<String, dynamic>?;
      final status = (data?['status'] as num?)?.toInt();
      return status == PaymentStatusApi.completed.value;
    } catch (_) {
      return false;
    }
  }

  /// Ako Stripe webhook nije dostupan, možete pozvati finalize sa session_id iz return URL-a.
  Future<bool> finalizeStripeSession(String sessionId) async {
    final uri = Uri.parse(
      '${ApiService.baseUrl}/Payments/stripe/finalize?session_id=${Uri.encodeQueryComponent(sessionId)}',
    );
    final token = await ApiService.getToken();
    final headers = {
      'Content-Type': 'application/json',
      if (token != null) 'Authorization': 'Bearer $token',
    };
    final response = await http.post(uri, headers: headers);
    if (response.statusCode != 200) return false;
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      final data = map['data'];
      return data == true;
    } catch (_) {
      return false;
    }
  }

  /// PayPal: nakon odobrenja u browseru (token = order id iz query parametra).
  Future<bool> capturePayPalOrder(String orderId) async {
    final response = await ApiService.post('/Payments/paypal/capture', {
      'token': orderId,
    });
    if (response.statusCode != 200 && response.statusCode != 201) {
      return false;
    }
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      return map['data'] == true;
    } catch (_) {
      return false;
    }
  }
}
