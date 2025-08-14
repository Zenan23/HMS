import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({Key? key}) : super(key: key);

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  String _email = '';
  String _password = '';
  bool _loading = false;
  String? _error;

  void _login(BuildContext context) async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    _formKey.currentState!.save();
    final auth = Provider.of<AuthService>(context, listen: false);
    final result = await auth.login(_email, _password);
    setState(() { _loading = false; });
    if (result == null) {
      Navigator.pushReplacementNamed(context, '/home');
    } else {
      setState(() { _error = result; });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text('Hotel Booking', style: Theme.of(context).textTheme.headlineLarge),
                const SizedBox(height: 32),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Email'),
                  keyboardType: TextInputType.emailAddress,
                  validator: (v) => v != null && v.contains('@') ? null : 'Unesite validan email',
                  onSaved: (v) => _email = v ?? '',
                ),
                const SizedBox(height: 16),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Lozinka'),
                  obscureText: true,
                  validator: (v) => v != null && v.length >= 6 ? null : 'Lozinka mora imati bar 6 karaktera',
                  onSaved: (v) => _password = v ?? '',
                ),
                const SizedBox(height: 24),
                if (_error != null) ...[
                  Text(_error!, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                ],
                _loading
                    ? const CircularProgressIndicator()
                    : ElevatedButton(
                        onPressed: () => _login(context),
                        child: const Text('Prijavi se'),
                      ),
                TextButton(
                  onPressed: () {
                    Navigator.pushReplacementNamed(context, '/register');
                  },
                  child: const Text('Nemate nalog? Registrujte se'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}