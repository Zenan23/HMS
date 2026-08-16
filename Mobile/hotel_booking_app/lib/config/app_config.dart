/// Centralna konfiguracija mobilne aplikacije.
///
/// Pokretanje s prilagođenom API adresom:
/// `flutter run --dart-define=baseUrl=http://192.168.1.10:8080/api`
class AppConfig {
  static const String baseUrl = String.fromEnvironment(
    'baseUrl',
    defaultValue: 'http://10.0.2.2:8080/api',
  );
}
