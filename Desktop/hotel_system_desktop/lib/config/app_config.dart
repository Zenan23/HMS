/// Centralna konfiguracija desktop aplikacije.
///
/// Pokretanje s prilagođenom API adresom:
/// `flutter run --dart-define=baseUrl=http://localhost:8080`
class AppConfig {
  static const String baseUrl = String.fromEnvironment(
    'baseUrl',
    defaultValue: 'http://localhost:8080',
  );
}
