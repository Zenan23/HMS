import 'package:flutter_stripe/flutter_stripe.dart' hide PaymentMethod, Card;

enum StripeCheckoutUi { paymentSheet, paymentElement, unsupported }

Future<void> initStripePlatform() async {}

Future<void> configureStripePublishableKey(String publishableKey) async {
  Stripe.publishableKey = publishableKey;
  // Obavezno za metode koje izlaze iz app-a (PayPal redirect, eventualni bank redirect za SEPA) —
  // bez ovoga Stripe SDK ne zna kako da vrati korisnika u app nakon vanjske autorizacije, pa
  // Payment Sheet ostane "zaglavljen" na Stripe-ovoj stranici (npr. nakon "Authorize Test Payment").
  Stripe.urlScheme = 'ebooking';
  await Stripe.instance.applySettings();
}

/// Deep link na koji Stripe (PayPal i sl.) vraća korisnika nakon vanjske autorizacije.
/// Mora biti registrovan kao poseban intent-filter (Android) — vidi AndroidManifest.xml,
/// host "stripe-redirect" — iOS scheme "ebooking" je već registrovan scheme-wide u Info.plist.
const String stripeReturnUrl = 'ebooking://stripe-redirect';

Future<void> presentStripePaymentSheet({
  required String clientSecret,
  required String merchantDisplayName,
}) async {
  await Stripe.instance.initPaymentSheet(
    paymentSheetParameters: SetupPaymentSheetParameters(
      paymentIntentClientSecret: clientSecret,
      merchantDisplayName: merchantDisplayName,
      returnURL: stripeReturnUrl,
    ),
  );
  await Stripe.instance.presentPaymentSheet();
}

/// Proslijedi dolazni deep link Stripe SDK-u — poziva se iz app_links listenera kad se korisnik
/// vrati iz vanjske autorizacije (npr. PayPal "Authorize Test Payment"). Vraća true ako je Stripe
/// prepoznao URL kao svoj (i time odblokirao `presentPaymentSheet()` poziv koji čeka na povratak).
Future<bool> handleStripeUrlCallback(String url) {
  return Stripe.instance.handleURLCallback(url);
}

Future<void> confirmStripePaymentElement({required String returnUrl}) async {
  throw UnsupportedError('Payment Element je samo za web.');
}

StripeCheckoutUi get stripeCheckoutUi => StripeCheckoutUi.paymentSheet;

bool get isStripeNativeSupported => true;
