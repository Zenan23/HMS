/// Deep link / return URL parametri nakon Stripe checkout-a.
/// Primjeri:
/// - ebooking://payment-return?paymentId=5&session_id=...
/// - http://10.0.2.2:8080/payment-return?paymentId=5&session_id=...
class PaymentReturnParams {
  final int? paymentId;
  final String? sessionId;
  final bool isCancel;

  const PaymentReturnParams({
    this.paymentId,
    this.sessionId,
    this.isCancel = false,
  });

  static const scheme = 'ebooking';

  static PaymentReturnParams? tryParse(Uri uri) {
    // 1) Custom scheme deep link
    if (uri.scheme == scheme) {
      return _fromDeepLinkHost(uri);
    }

    // 2) HTTP/HTTPS API fallback return host (ebooking.app)
    final path = uri.path.toLowerCase();
    final isReturnPath =
        path.endsWith('/payment-return') || path == '/payment-return';
    final isCancelPath =
        path.endsWith('/payment-cancel') || path == '/payment-cancel';
    if (isReturnPath || isCancelPath) {
      return PaymentReturnParams(
        paymentId: int.tryParse(uri.queryParameters['paymentId'] ?? ''),
        sessionId: uri.queryParameters['session_id'],
        isCancel: isCancelPath,
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
}
