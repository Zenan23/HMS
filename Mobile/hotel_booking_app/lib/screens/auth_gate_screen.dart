import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';
import 'home_screen.dart';
import 'login_screen.dart';

/// Pokreće se prvo (umjesto direktno na LoginScreen) i pokušava vratiti sesiju iz
/// FlutterSecureStorage (AuthService.tryAutoLogin) prije nego odluči da li ide na
/// login ili home. Token je već perzistiran (flutter_secure_storage) — bez ovog
/// gate ekrana AuthService._user ostaje null pri svakom novom pokretanju Flutter
/// engine-a, pa korisnik uvijek završi na login ekranu čak i kad je token validan
/// (npr. kad Android ubije/rekreira proces u pozadini dok je Payment Sheet otvorio
/// Chrome Custom Tab za PayPal — ista sesija bi trebala preživjeti taj povratak).
class AuthGateScreen extends StatefulWidget {
  const AuthGateScreen({super.key});

  @override
  State<AuthGateScreen> createState() => _AuthGateScreenState();
}

class _AuthGateScreenState extends State<AuthGateScreen> {
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _check());
  }

  Future<void> _check() async {
    final auth = Provider.of<AuthService>(context, listen: false);
    await auth.tryAutoLogin();
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(
        builder: (_) =>
            auth.user != null ? const HomeScreen() : const LoginScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(child: CircularProgressIndicator()),
    );
  }
}
