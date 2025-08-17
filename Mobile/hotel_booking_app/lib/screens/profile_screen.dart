import 'package:flutter/material.dart';
import 'package:hotel_booking_app/models/user.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';
import '../utils/validation_utils.dart';
import 'dart:convert';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({Key? key}) : super(key: key);

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _email;
  late TextEditingController _username;
  late TextEditingController _firstName;
  late TextEditingController _lastName;
  late TextEditingController _phoneNumber;
  bool _saving = false;
  String? _status;
  bool _editing = false;

  @override
  void initState() {
    super.initState();
    final user = context.read<AuthService>().user;
    _email = TextEditingController(text: user?.email ?? '');
    _username = TextEditingController(text: user?.username ?? '');
    _firstName = TextEditingController(text: user?.firstName ?? '');
    _lastName = TextEditingController(text: user?.lastName ?? '');
    _phoneNumber = TextEditingController(text: user?.phoneNumber ?? '');
  }

  @override
  void dispose() {
    _email.dispose();
    _username.dispose();
    _firstName.dispose();
    _lastName.dispose();
    _phoneNumber.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    final auth = context.read<AuthService>();
    final user = auth.user!;
    setState(() {
      _saving = true;
      _status = null;
    });
    try {
      final resp = await ApiService.put('/Users/${user.userId}', {
        'id': user.userId,
        'username': _username.text.trim(),
        'email': _email.text.trim(),
        'firstName': _firstName.text.trim(),
        'lastName': _lastName.text.trim(),
        'phoneNumber': _phoneNumber.text.trim(),
        'role': user.role,
        'isActive': true,
      });
      if (resp.statusCode == 200) {
        final data = jsonDecode(resp.body);
        final updated = data['data'] ?? data;
        final updatedUser = User(
          userId: user.userId,
          token: user.token,
          email: updated['email'] ?? _email.text.trim(),
          username: updated['username'] ?? _username.text.trim(),
          firstName: updated['firstName'] ?? _firstName.text.trim(),
          lastName: updated['lastName'] ?? _lastName.text.trim(),
          phoneNumber: updated['phoneNumber'] ?? _phoneNumber.text.trim(),
          role: user.role,
          expiresAt: user.expiresAt,
        );
        auth.updateLocalUser(updatedUser);
        setState(() {
          _status = 'Sačuvano.';
        });
      } else {
        setState(() {
          _status = 'Greška pri čuvanju.';
        });
      }
    } catch (_) {
      setState(() {
        _status = 'Greška pri povezivanju sa serverom.';
      });
    } finally {
      setState(() {
        _saving = false;
      });
    }
  }

@override
Widget build(BuildContext context) {
  final auth = context.watch<AuthService>();
  final user = auth.user;
  if (user == null) {
    return const Scaffold(body: Center(child: Text('Niste prijavljeni.')));
  }

  return Scaffold(
    appBar: AppBar(
      title: const Text('Profil'),
      centerTitle: true,
      elevation: 2,
    ),
    body: SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Card(
        elevation: 4,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _buildTextField(
                  controller: _email,
                  label: 'Email',
                  icon: Icons.email,
                  enabled: _editing,
                  keyboardType: TextInputType.emailAddress,
                  validator: ValidationUtils.validateEmail,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _username,
                  label: 'Korisničko ime',
                  icon: Icons.person,
                  enabled: _editing,
                  validator: ValidationUtils.validateUsername,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _firstName,
                  label: 'Ime',
                  icon: Icons.badge,
                  enabled: _editing,
                  validator: ValidationUtils.validateFirstName,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _lastName,
                  label: 'Prezime',
                  icon: Icons.badge_outlined,
                  enabled: _editing,
                  validator: ValidationUtils.validateLastName,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _phoneNumber,
                  label: 'Telefon',
                  icon: Icons.phone,
                  enabled: _editing,
                  keyboardType: TextInputType.phone,
                  validator: ValidationUtils.validatePhoneNumber,
                ),
                const SizedBox(height: 24),
                if (_status != null) ...[
                  Text(
                    _status!,
                    style: TextStyle(
                      color: _status == 'Sačuvano.' ? Colors.green : Colors.red,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 12),
                ],
                Row(
                  children: [
                    Expanded(
                      child: ElevatedButton.icon(
                        onPressed: _editing
                            ? (_saving ? null : _save)
                            : () {
                                setState(() {
                                  _editing = true;
                                });
                              },
                        icon: _editing
                            ? const Icon(Icons.save)
                            : const Icon(Icons.edit),
                        label: _saving
                            ? const CircularProgressIndicator()
                            : Text(_editing ? 'Sačuvaj' : 'Uredi'),
                        style: ElevatedButton.styleFrom(
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(8)),
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    ElevatedButton.icon(
                      onPressed: () {
                        auth.logout();
                        Navigator.pushReplacementNamed(context, '/login');
                      },
                      icon: const Icon(Icons.logout),
                      label: const Text('Odjavi se'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.red,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8)),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    ),
  );
}

Widget _buildTextField({
  required TextEditingController controller,
  required String label,
  required IconData icon,
  bool enabled = true,
  TextInputType? keyboardType,
  String? Function(String?)? validator,
}) {
  return TextFormField(
    controller: controller,
    decoration: InputDecoration(
      labelText: label,
      prefixIcon: Icon(icon),
      filled: true,
      fillColor: Colors.grey.shade100,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
      ),
    ),
    keyboardType: keyboardType,
    validator: validator,
    enabled: enabled,
  );
}

}