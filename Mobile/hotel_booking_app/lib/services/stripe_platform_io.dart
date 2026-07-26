import 'package:flutter_stripe/flutter_stripe.dart' hide PaymentMethod, Card;

enum StripeCheckoutUi { paymentSheet, paymentElement, unsupported }

Future<void> initStripePlatform() async {
  Stripe.merchantIdentifier = 'merchant.com.example.hotel_booking_app';
}

Future<void> configureStripePublishableKey(String publishableKey) async {
  Stripe.publishableKey = publishableKey;
  await Stripe.instance.applySettings();
}

Future<void> presentStripePaymentSheet({
  required String clientSecret,
  required String merchantDisplayName,
}) async {
  await Stripe.instance.initPaymentSheet(
    paymentSheetParameters: SetupPaymentSheetParameters(
      paymentIntentClientSecret: clientSecret,
      merchantDisplayName: merchantDisplayName,
    ),
  );
  await Stripe.instance.presentPaymentSheet();
}

Future<void> confirmStripePaymentElement({required String returnUrl}) async {
  throw UnsupportedError('Payment Element je samo za web.');
}

StripeCheckoutUi get stripeCheckoutUi => StripeCheckoutUi.paymentSheet;

bool get isStripeNativeSupported => true;
