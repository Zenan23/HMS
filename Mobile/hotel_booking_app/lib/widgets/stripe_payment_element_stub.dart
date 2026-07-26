import 'package:flutter/material.dart';

/// Stub (non-web): Payment Element nije dostupan.
class StripePaymentElementView extends StatelessWidget {
  final String clientSecret;
  final ValueChanged<bool>? onCardComplete;

  const StripePaymentElementView({
    super.key,
    required this.clientSecret,
    this.onCardComplete,
  });

  @override
  Widget build(BuildContext context) {
    return const SizedBox.shrink();
  }
}

Future<void> confirmWebPaymentElement({required String returnUrl}) async {
  throw UnsupportedError('Payment Element je dostupan samo na webu.');
}

String currentPageUrl() => '';

