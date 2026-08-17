// Osnovni smoke test za Hotel Booking mobilnu aplikaciju.
//
// Provjerava da se aplikacija uspješno pokrene i da se prikaže login ekran
// (initialRoute '/login') sa očekivanim elementima, bez mrežnih poziva.

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:hotel_booking_app/main.dart';

void main() {
  testWidgets('Aplikacija se pokreće i prikazuje login ekran', (WidgetTester tester) async {
    // Build aplikacije i triggerovanje frame-a.
    await tester.pumpWidget(const MyApp());
    await tester.pumpAndSettle();

    // Naslov aplikacije treba biti vidljiv na login ekranu.
    expect(find.text('Hotel Booking'), findsOneWidget);

    // Forma za prijavu: polja za email/lozinku i dugme za prijavu.
    expect(find.widgetWithText(ElevatedButton, 'Prijavi se'), findsOneWidget);
    expect(find.byType(TextFormField), findsNWidgets(2));

    // Linkovi ka registraciji i resetu lozinke moraju postojati.
    expect(find.text('Nemate nalog? Registrujte se'), findsOneWidget);
    expect(find.text('Zaboravili ste lozinku?'), findsOneWidget);
  });
}
