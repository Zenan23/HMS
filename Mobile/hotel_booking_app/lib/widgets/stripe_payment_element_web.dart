import 'package:flutter/material.dart';
import 'package:flutter_stripe_web/flutter_stripe_web.dart';
import 'package:web/web.dart' as web;

/// Ugrađeni Stripe Payment Element (web).
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
    return PaymentElement(
      autofocus: true,
      enablePostalCode: true,
      clientSecret: clientSecret,
      onCardChanged: (details) {
        onCardComplete?.call(details?.complete == true);
      },
    );
  }
}

Future<void> confirmWebPaymentElement({required String returnUrl}) async {
  await WebStripe.instance.confirmPaymentElement(
    ConfirmPaymentElementOptions(
      confirmParams: ConfirmPaymentParams(return_url: returnUrl),
      redirect: PaymentConfirmationRedirect.ifRequired,
    ),
  );
}

String currentPageUrl() => web.window.location.href;
