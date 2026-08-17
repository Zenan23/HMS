import 'package:flutter/foundation.dart' show kIsWeb;

/// Centralna konfiguracija mobilne aplikacije.
///
/// Pokretanje s prilagođenom API adresom:
/// `flutter run --dart-define=API_BASE_URL=http://192.168.1.10:8080/api`
class AppConfig {
  // 10.0.2.2 je poseban alias koji radi SAMO unutar Android emulatora (mapira se na
  // localhost host mašine). Flutter web (Chrome) se izvršava direktno na host mašini, pa
  // tamo mora ići pravi localhost — inače dobijaš net::ERR_NETWORK_CHANGED / connection
  // refused jer 10.0.2.2 ne vodi nigdje van emulatora.
  static const String _defaultBaseUrl =
      kIsWeb ? 'http://localhost:8080/api' : 'http://10.0.2.2:8080/api';

  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: _defaultBaseUrl,
  );
}
