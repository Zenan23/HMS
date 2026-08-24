import 'dart:async';
import 'dart:io';
import 'package:flutter/material.dart';
import '../utils/api_response.dart';

/// Pretvara bilo koju grešku (iz API poziva, mreže, parsiranja, ili bilo šta
/// neočekivano) u čistu poruku na bosanskom jeziku, razumljivu korisniku.
/// Nikad ne vraća sirovi `Exception: ...`/stack trace tekst — to koristi
/// SVAKO mjesto u aplikaciji koje prikazuje grešku korisniku (SnackBar,
/// inline tekst u formama, itd.), umjesto direktnog `error.toString()`.
String friendlyErrorMessage(Object error) {
  if (error is ApiException) {
    return error.isForbidden ? 'Nemate dozvolu za ovu akciju.' : error.message;
  }
  if (error is SocketException) {
    return 'Nije moguće povezati se sa serverom. Provjerite internet konekciju.';
  }
  if (error is TimeoutException) {
    return 'Isteklo je vrijeme čekanja odgovora servera. Pokušajte ponovo.';
  }
  if (error is HttpException) {
    return 'Greška pri komunikaciji sa serverom.';
  }
  if (error is FormatException) {
    return 'Neočekivan odgovor sa servera.';
  }
  // Obični Exception('poruka') (npr. iz AuthService.login) — Dart-ov
  // Exception.toString() dodaje "Exception: " prefiks koji korisnik ne treba
  // vidjeti, pa se ovdje skida prije prikaza. Bez ove grane, konkretna poruka
  // sa backend-a (npr. "Pogrešan email ili lozinka.") nikad ne bi stigla do
  // korisnika — pao bi na generički fallback ispod.
  if (error is Exception) {
    final text = error.toString();
    // Dart-ov ugrađeni Exception.toString() UVIJEK dodaje baš "Exception: "
    // prefiks (fiksan engleski tekst iz Dart core biblioteke, ne nešto što
    // mi biramo) — zato se provjerava tačno taj string, ne "Greška: ".
    return text.startsWith('Exception: ') ? text.substring(11) : text;
  }
  return 'Došlo je do neočekivane greške. Pokušajte ponovo.';
}

void showApiError(BuildContext context, Object error) {
  final message = friendlyErrorMessage(error);
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Text(message),
      backgroundColor: error is ApiException && error.isForbidden
          ? Colors.orange
          : Colors.red,
    ),
  );
}
