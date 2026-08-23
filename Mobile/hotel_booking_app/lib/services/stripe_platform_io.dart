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
      // SEPA Direct Debit je "delayed notification" metoda plaćanja — mandat se potvrdi odmah,
      // ali stvarna naplata je asinhrona (u test modu zna potrajati par minuta). Payment Sheet
      // po defaultu SAKRIJE takve metode iz liste (allowsDelayedPaymentMethods = false), čak i
      // kad su navedene u PaymentIntent.payment_method_types. Bez ovoga korisnik nikad ne bi
      // vidio SEPA opciju. UI (Reservations ekran, payment_screen.dart) eksplicitno prikazuje
      // "u obradi" umjesto greške dok se SEPA plaćanje ne potvrdi — vidi PAYMENT_INTEGRATION.md.
      allowsDelayedPaymentMethods: true,
      // Podrazumijevano Payment Sheet traži barem "Country or region" + "ZIP Code" za karticu
      // (AVS fraud provjera) i puni naziv/email za SEPA (mandat). To je dodatno trenje koje nam
      // ne treba za ovaj projekat — isključeno je u potpunosti (nema polja za adresu). SEPA i
      // dalje traži ime/email (to su odvojena polja, name/email CollectionMode, ne address —
      // ostaju na "automatic"), samo puna poštanska adresa nije obavezna ni za karticu ni za
      // SEPA. Ako se ikad ispostavi da Stripe test nalog ipak insistira na adresi za neku
      // metodu, prvi korak za dijagnozu je vratiti ovo na AddressCollectionMode.automatic.
      billingDetailsCollectionConfiguration: const BillingDetailsCollectionConfiguration(
        address: AddressCollectionMode.never,
      ),
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
