import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  String _email = '';
  String _password = '';

  @override
  Widget build(BuildContext context) {
    final auth = Provider.of<AuthProvider>(context);
    return Scaffold(
      body: Center(
        child: Card(
          elevation: 8,
          child: Padding(
            padding: const EdgeInsets.all(32.0),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Text('Prijava', style: TextStyle(fontSize: 24)),
                  TextFormField(
                    decoration: const InputDecoration(labelText: 'Email'),
                    onChanged: (v) => _email = v,
                    keyboardType: TextInputType.emailAddress,
                  ),
                  TextFormField(
                    decoration: const InputDecoration(labelText: 'Lozinka'),
                    obscureText: true,
                    onChanged: (v) => _password = v,
                  ),
                  const SizedBox(height: 16),
                  if (auth.error != null) Text(auth.error!, style: const TextStyle(color: Colors.red)),
                  ElevatedButton(
                    onPressed: auth.isLoading
                        ? null
                        : () {
                            if (_formKey.currentState!.validate()) {
                              auth.login(_email, _password);
                            }
                          },
                    child: auth.isLoading
                        ? const CircularProgressIndicator()
                        : const Text('Prijavi se'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}