/// Centralna konfiguracija mobilne aplikacije.
///
/// Pokretanje s prilagođenom API adresom:
/// `flutter run --dart-define=API_BASE_URL=http://192.168.1.10:8080/api`
class AppConfig {
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:8080/api',
  );
}
