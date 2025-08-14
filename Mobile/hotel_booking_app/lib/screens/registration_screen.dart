import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';

class RegistrationScreen extends StatefulWidget {
  const RegistrationScreen({Key? key}) : super(key: key);

  @override
  State<RegistrationScreen> createState() => _RegistrationScreenState();
}

class _RegistrationScreenState extends State<RegistrationScreen> {
  final _formKey = GlobalKey<FormState>();
  String _username = '';
  String _email = '';
  String _password = '';
  String _confirmPassword = '';
  String _firstName = '';
  String _lastName = '';
  String _phoneNumber = '';
  bool _loading = false;
  String? _error;

  void _register(BuildContext context) async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    _formKey.currentState!.save();
    final auth = Provider.of<AuthService>(context, listen: false);
    final result = await auth.register(
      username: _username,
      email: _email,
      password: _password,
      confirmPassword: _confirmPassword,
      firstName: _firstName,
      lastName: _lastName,
      phoneNumber: _phoneNumber,
    );
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
      appBar: AppBar(title: const Text('Registracija')),
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text('Kreiraj nalog', style: Theme.of(context).textTheme.headlineSmall),
                const SizedBox(height: 32),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Korisničko ime'),
                  maxLength: 50,
                  validator: (v) => v == null || v.isEmpty ? 'Unesite korisničko ime' : (v.length < 3 ? 'Korisničko ime mora imati bar 3 karaktera' : null),
                  onSaved: (v) => _username = v ?? '',
                ),
                const SizedBox(height: 8),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Email'),
                  keyboardType: TextInputType.emailAddress,
                  maxLength: 100,
                  validator: (v) {
                    if (v == null || v.isEmpty) return 'Unesite email';
                    if (!v.contains('@')) return 'Unesite validan email';
                    return null;
                  },
                  onSaved: (v) => _email = v ?? '',
                ),
                const SizedBox(height: 8),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Lozinka'),
                  obscureText: true,
                  maxLength: 100,
                  onChanged: (v) => setState(() => _password = v),
                  validator: (v) => v == null || v.length < 6 ? 'Lozinka mora imati bar 6 karaktera' : null,
                  onSaved: (v) => _password = v ?? '',
                ),
                const SizedBox(height: 8),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Potvrdi lozinku'),
                  obscureText: true,
                  maxLength: 100,
                  validator: (v) => v == null || v != _password ? 'Lozinke se ne poklapaju' : null,
                  onSaved: (v) => _confirmPassword = v ?? '',
                ),
                const SizedBox(height: 8),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Ime'),
                  maxLength: 50,
                  validator: (v) => v == null || v.isEmpty ? 'Unesite ime' : null,
                  onSaved: (v) => _firstName = v ?? '',
                ),
                const SizedBox(height: 8),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Prezime'),
                  maxLength: 50,
                  validator: (v) => v == null || v.isEmpty ? 'Unesite prezime' : null,
                  onSaved: (v) => _lastName = v ?? '',
                ),
                const SizedBox(height: 8),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Broj telefona'),
                  maxLength: 20,
                  keyboardType: TextInputType.phone,
                  validator: (v) {
                    if (v == null || v.isEmpty) return null;
                    final phoneRegExp = RegExp(r'^[+]?\d{7,20}$');
                    if (!phoneRegExp.hasMatch(v)) return 'Unesite validan broj telefona';
                    return null;
                  },
                  onSaved: (v) => _phoneNumber = v ?? '',
                ),
                const SizedBox(height: 24),
                if (_error != null) ...[
                  Text(_error!, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                ],
                _loading
                    ? const CircularProgressIndicator()
                    : ElevatedButton(
                        onPressed: () => _register(context),
                        child: const Text('Registruj se'),
                      ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}