/// Deep link / return URL parametri nakon Stripe/PayPal checkout-a.
/// Primjeri:
/// - ebooking://payment-return?paymentId=5&token=...
/// - http://10.0.2.2:8080/payment-return?paymentId=5&token=...
/// - PayPal genericError?...&token=...&cancelLink=ebooking://payment-return?...
class PaymentReturnParams {
  final int? paymentId;
  final String? sessionId;
  final String? payPalToken;
  final bool isCancel;

  const PaymentReturnParams({
    this.paymentId,
    this.sessionId,
    this.payPalToken,
    this.isCancel = false,
  });

  static const scheme = 'ebooking';

  static PaymentReturnParams? tryParse(Uri uri) {
    // 1) Custom scheme deep link
    if (uri.scheme == scheme) {
      return _fromDeepLinkHost(uri);
    }

    // 2) HTTP/HTTPS API fallback / WebView return host (ebooking.app)
    final path = uri.path.toLowerCase();
    final isReturnPath =
        path.endsWith('/payment-return') || path == '/payment-return';
    final isCancelPath =
        path.endsWith('/payment-cancel') || path == '/payment-cancel';
    if (isReturnPath || isCancelPath) {
      return PaymentReturnParams(
        paymentId: int.tryParse(uri.queryParameters['paymentId'] ?? ''),
        sessionId: uri.queryParameters['session_id'],
        payPalToken: uri.queryParameters['token'],
        isCancel: isCancelPath,
      );
    }

    // 3) PayPal genericError — često sadrži token + cancelLink s PayerID (odobreno, ali deep link pao)
    final host = uri.host.toLowerCase();
    final isPayPalError = host.contains('paypal.com') &&
        (path.contains('genericerror') || path.contains('genericError'));
    if (isPayPalError || uri.queryParameters.containsKey('cancelLink')) {
      final fromCancelLink = _fromCancelLink(uri.queryParameters['cancelLink']);
      if (fromCancelLink != null) return fromCancelLink;

      final token = uri.queryParameters['token'];
      if (token != null && token.isNotEmpty) {
        // Ako ima PayerID u cancelLink-u ili queryju — tretiraj kao uspješan return.
        final payerId = uri.queryParameters['PayerID'];
        if (payerId != null && payerId.isNotEmpty) {
          return PaymentReturnParams(payPalToken: token);
        }
        // genericError s tokenom — ipak pokušaj capture (order je često APPROVED)
        return PaymentReturnParams(payPalToken: token);
      }
    }

    // 4) Bilo koji URL s PayPal token + PayerID (uspješno odobrenje)
    final token = uri.queryParameters['token'];
    final payerId = uri.queryParameters['PayerID'];
    if (token != null &&
        token.isNotEmpty &&
        payerId != null &&
        payerId.isNotEmpty) {
      return PaymentReturnParams(
        paymentId: int.tryParse(uri.queryParameters['paymentId'] ?? ''),
        payPalToken: token,
      );
    }

    return null;
  }

  static PaymentReturnParams? _fromDeepLinkHost(Uri uri) {
    final host = uri.host.toLowerCase();
    final paymentId = int.tryParse(uri.queryParameters['paymentId'] ?? '');

    if (host == 'payment-return') {
      return PaymentReturnParams(
        paymentId: paymentId,
        sessionId: uri.queryParameters['session_id'],
        payPalToken: uri.queryParameters['token'],
      );
    }
    if (host == 'payment-cancel') {
      return PaymentReturnParams(
        paymentId: paymentId,
        isCancel: true,
      );
    }
    return null;
  }

  static PaymentReturnParams? _fromCancelLink(String? cancelLink) {
    if (cancelLink == null || cancelLink.isEmpty) return null;
    final decoded = Uri.decodeComponent(cancelLink);
    final nested = Uri.tryParse(decoded);
    if (nested == null) return null;
    if (nested.scheme == scheme) {
      return _fromDeepLinkHost(nested);
    }
    // Nested može biti i HTTP payment-return
    return tryParse(nested);
  }
}
