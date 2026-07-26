enum StripeCheckoutUi { paymentSheet, paymentElement, unsupported }

Future<void> initStripePlatform() async {}

Future<void> configureStripePublishableKey(String publishableKey) async {}

Future<void> presentStripePaymentSheet({
  required String clientSecret,
  required String merchantDisplayName,
}) async {
  throw UnsupportedError('Stripe Payment Sheet nije podržan na ovoj platformi.');
}

Future<void> confirmStripePaymentElement({required String returnUrl}) async {
  throw UnsupportedError('Stripe Payment Element nije podržan na ovoj platformi.');
}

StripeCheckoutUi get stripeCheckoutUi => StripeCheckoutUi.unsupported;

bool get isStripeNativeSupported => false;
