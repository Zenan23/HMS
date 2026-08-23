import 'package:flutter_stripe/flutter_stripe.dart' hide PaymentMethod, Card;

enum StripeCheckoutUi { paymentSheet, paymentElement, unsupported }

Future<void> initStripePlatform() async {
  // Web: bez Apple Pay merchant identifiera.
}

Future<void> configureStripePublishableKey(String publishableKey) async {
  Stripe.publishableKey = publishableKey;
  await Stripe.instance.applySettings();
}

Future<void> presentStripePaymentSheet({
  required String clientSecret,
  required String merchantDisplayName,
}) async {
  throw UnsupportedError('Na webu koristite Payment Element, ne Payment Sheet.');
}

Future<void> confirmStripePaymentElement({required String returnUrl}) async {
  // Deferred to stripe_payment_form_web via WebStripe to avoid circular deps.
  throw UnimplementedError('Koristite confirmWebPaymentElement iz stripe_payment_form.');
}

const String stripeReturnUrl = '';

/// Web ne koristi Payment Sheet (vidi presentStripePaymentSheet iznad) pa nema deep link povratak
/// za obraditi — Payment Element radi kroz redirect na stranici, ne kroz app_links.
Future<bool> handleStripeUrlCallback(String url) async => false;

StripeCheckoutUi get stripeCheckoutUi => StripeCheckoutUi.paymentElement;

bool get isStripeNativeSupported => true;
