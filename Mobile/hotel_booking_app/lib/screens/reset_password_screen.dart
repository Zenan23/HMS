import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';

/// Drugi korak reset lozinke: korisnik unosi kod primljen emailom i novu lozinku.
class ResetPasswordScreen extends StatefulWidget {
  final String email;

  const ResetPasswordScreen({super.key, required this.email});

  @override
  State<ResetPasswordScreen> createState() => _ResetPasswordScreenState();
}

class _ResetPasswordScreenState extends State<ResetPasswordScreen> {
  final _formKey = GlobalKey<FormState>();
  String _code = '';
  String _newPassword = '';
  String _confirmPassword = '';
  bool _loading = false;
  String? _error;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    _formKey.currentState!.save();

    final auth = Provider.of<AuthService>(context, listen: false);
    final result = await auth.resetPassword(
      email: widget.email,
      code: _code,
      newPassword: _newPassword,
      confirmNewPassword: _confirmPassword,
    );

    if (!mounted) return;
    setState(() => _loading = false);

    if (result == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Lozinka je uspješno promijenjena. Prijavite se novom lozinkom.'),
          backgroundColor: Colors.green,
        ),
      );
      Navigator.of(context).pushNamedAndRemoveUntil('/login', (route) => false);
    } else {
      setState(() => _error = result);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Unesite kod')),
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(
                  'Kod je poslan na ${widget.email}',
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 24),
                TextFormField(
                  decoration: const InputDecoration(
                    labelText: 'Kod (6 cifara)',
                    counterText: '',
                  ),
                  keyboardType: TextInputType.number,
                  maxLength: 6,
                  validator: (v) => v != null && v.length == 6
                      ? null
                      : 'Unesite 6-cifreni kod iz emaila',
                  onSaved: (v) => _code = (v ?? '').trim(),
                ),
                const SizedBox(height: 16),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Nova lozinka'),
                  obscureText: true,
                  validator: (v) => v != null && v.length >= 6
                      ? null
                      : 'Lozinka mora imati bar 6 karaktera',
                  onSaved: (v) => _newPassword = v ?? '',
                ),
                const SizedBox(height: 16),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Potvrdite novu lozinku'),
                  obscureText: true,
                  validator: (v) =>
                      v == _newPassword ? null : 'Lozinke se ne poklapaju',
                  onSaved: (v) => _confirmPassword = v ?? '',
                ),
                const SizedBox(height: 24),
                if (_error != null) ...[
                  Text(_error!, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                ],
                _loading
                    ? const Center(child: CircularProgressIndicator())
                    : ElevatedButton(
                        onPressed: _submit,
                        child: const Text('Postavi novu lozinku'),
                      ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
