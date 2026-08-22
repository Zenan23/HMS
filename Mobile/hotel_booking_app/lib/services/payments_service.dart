import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/payment.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class PaymentsService {
  PaymentConfig? _cachedConfig;

  /// Konfiguracija plaćanja (native vs hosted, Stripe publishable key).
  Future<PaymentConfig?> getPaymentConfig({bool forceRefresh = false}) async {
    if (_cachedConfig != null && !forceRefresh) return _cachedConfig;

    final response = await ApiService.get('/Payments/config');
    if (response.statusCode != 200) return null;
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      final data = map['data'] as Map<String, dynamic>?;
      if (data == null) return null;
      _cachedConfig = PaymentConfig.fromJson(data);
      return _cachedConfig;
    } catch (_) {
      return null;
    }
  }

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

  /// Stripe PaymentIntent za in-app Payment Sheet.
  Future<StripeIntentResponse?> startStripeIntent(CreateHostedCheckoutDto dto) async {
    final response = await ApiService.post('/Payments/stripe/intent', dto.toJson());
    if (response.statusCode != 200 && response.statusCode != 201) {
      return null;
    }
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      final data = map['data'] as Map<String, dynamic>?;
      if (data == null) return null;
      return StripeIntentResponse.fromJson(data);
    } catch (_) {
      return null;
    }
  }

  /// Sva plaćanja vezana za rezervaciju (koristi se za pronalazak plaćanja koje se refundira).
  Future<List<PaymentDetails>> getPaymentsByBooking(int bookingId) async {
    final response = await ApiService.get('/Payments/booking/$bookingId');
    return ApiResponseParser.parseList(response, PaymentDetails.fromJson);
  }

  /// Otkazuje plaćanje koje je ostalo "zaglavljeno" u statusu Processing — npr. kad korisnik
  /// odustane/otkaže na Stripe strani, ili capture/confirm ne uspije. Backend blokira
  /// pokretanje novog checkout-a za istu rezervaciju dok god postoji Processing/Completed
  /// plaćanje (vidi ValidateAndPrepareCheckoutAsync), pa je ovaj poziv OBAVEZAN nakon svakog
  /// otkazanog/neuspjelog pokušaja da korisnik može ponovo platiti. Best-effort — greška ovdje
  /// se ne prikazuje korisniku (ionako je već na "failed" ekranu), samo se loguje tiho.
  Future<bool> cancelPayment(int paymentId, String reason) async {
    try {
      final response = await ApiService.post('/Payments/$paymentId/cancel', {
        'reason': reason,
      });
      return response.statusCode == 200 || response.statusCode == 201;
    } catch (_) {
      return false;
    }
  }

  /// Zahtjev za povrat novca za dato plaćanje. `amount` je iznos koji se refundira (obično puni
  /// iznos plaćanja), `reason` je obavezan razlog koji se šalje serveru i upisuje u audit log.
  Future<bool> refundPayment(int paymentId, num amount, String reason) async {
    final response = await ApiService.post('/Payments/$paymentId/refund', {
      'amount': amount,
      'reason': reason,
    });
    ApiResponseParser.ensureSuccess(response);
    return true;
  }

  Future<PaymentDetails?> getPaymentDetails(int paymentId) async {
    final response = await ApiService.get('/Payments/$paymentId');
    if (response.statusCode != 200) return null;
    try {
      final map = jsonDecode(response.body) as Map<String, dynamic>;
      final data = map['data'] as Map<String, dynamic>?;
      if (data == null) return null;
      return PaymentDetails.fromJson(data);
    } catch (_) {
      return null;
    }
  }

  /// Provjera statusa plaćanja (polling). Vraća true ako je completed (3).
  Future<bool> isPaymentCompleted(int paymentId) async {
    final details = await getPaymentDetails(paymentId);
    return details?.isCompleted ?? false;
  }

  /// Nakon povratka iz preglednika: Stripe finalize preko checkoutId.
  Future<void> confirmPaymentAfterReturn(int paymentId, PaymentMethod method) async {
    if (await isPaymentCompleted(paymentId)) return;

    final details = await getPaymentDetails(paymentId);
    if (details == null || !details.isPendingConfirmation) return;

    final checkoutId = details.checkoutId?.trim();
    if (checkoutId == null || checkoutId.isEmpty) return;

    switch (method) {
      case PaymentMethod.stripe:
        if (checkoutId.startsWith('pi_')) {
          await confirmStripePaymentIntent(checkoutId);
        } else {
          await finalizeStripeSession(checkoutId);
        }
        break;
    }
  }

  /// Potvrda Stripe PaymentIntent-a nakon Payment Sheet-a.
  Future<bool> confirmStripePaymentIntent(String paymentIntentId) async {
    final uri = Uri.parse(
      '${ApiService.baseUrl}/Payments/stripe/confirm?payment_intent_id=${Uri.encodeQueryComponent(paymentIntentId)}',
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

  /// Čeka da backend potvrdi plaćanje (polling).
  Future<bool> waitForPaymentCompletion(
    int paymentId, {
    PaymentMethod? method,
    int maxAttempts = 30,
    Duration interval = const Duration(seconds: 2),
  }) async {
    final svc = this;
    for (var i = 0; i < maxAttempts; i++) {
      if (await svc.isPaymentCompleted(paymentId)) return true;

      if (method != null && i > 0 && i % 5 == 0) {
        await svc.confirmPaymentAfterReturn(paymentId, method);
      }
      await Future.delayed(interval);
    }
    return await svc.isPaymentCompleted(paymentId);
  }
}
