import 'package:flutter/material.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../utils/validation_utils.dart';
import 'dart:convert';

class EmployeeFormDialog extends StatefulWidget {
  final Employee? employee;
  final bool forceRoleEmployee;
  const EmployeeFormDialog({super.key, this.employee, this.forceRoleEmployee = false});

  @override
  State<EmployeeFormDialog> createState() => _EmployeeFormDialogState();
}

class _EmployeeFormDialogState extends State<EmployeeFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late int id;
  late String username;
  late String email;
  late String password;
  late String firstName;
  late String lastName;
  late String phoneNumber;
  UserRole role = UserRole.Employee;
  bool isActive = true;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final e = widget.employee;
    id = e?.id ?? 0;
    username = e?.username ?? '';
    email = e?.email ?? '';
    password = '';
    firstName = e?.firstName ?? '';
    lastName = e?.lastName ?? '';
    phoneNumber = e?.phoneNumber ?? '';
    role = widget.forceRoleEmployee ? UserRole.Employee : (e?.role ?? UserRole.Employee);
    isActive = e?.isActive ?? true;
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { isLoading = true; error = null; });
    final body = {
      'id': id,
      'username': username,
      'email': email,
      'password': password.isNotEmpty ? password : null,
      'firstName': firstName,
      'lastName': lastName,
      'phoneNumber': phoneNumber,
      'role': userRoleToInt(role),
      'isActive': isActive,
    }..removeWhere((k, v) => v == null);
    try {
      if (widget.employee == null) {
        await ApiService().post('/api/Users', body);
      } else {
        await ApiService().put('/api/Users/${widget.employee!.id}', body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() { error = e.toString(); });
    }
    setState(() { isLoading = false; });
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.employee == null ? 'Dodaj uposlenika' : 'Uredi uposlenika'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: username,
                decoration: const InputDecoration(labelText: 'Korisničko ime'),
                onChanged: (v) => username = v,
                validator: ValidationUtils.validateUsername,
              ),
              TextFormField(
                initialValue: email,
                decoration: const InputDecoration(labelText: 'Email'),
                onChanged: (v) => email = v,
                validator: ValidationUtils.validateEmail,
              ),
              if (widget.employee == null)
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Lozinka'),
                  obscureText: true,
                  onChanged: (v) => password = v,
                  validator: ValidationUtils.validatePassword,
                ),
              TextFormField(
                initialValue: firstName,
                decoration: const InputDecoration(labelText: 'Ime'),
                onChanged: (v) => firstName = v,
                validator: ValidationUtils.validateFirstName,
              ),
              TextFormField(
                initialValue: lastName,
                decoration: const InputDecoration(labelText: 'Prezime'),
                onChanged: (v) => lastName = v,
                validator: ValidationUtils.validateLastName,
              ),
              TextFormField(
                initialValue: phoneNumber,
                decoration: const InputDecoration(labelText: 'Telefon'),
                onChanged: (v) => phoneNumber = v,
                validator: ValidationUtils.validatePhoneNumber,
              ),
              DropdownButtonFormField<UserRole>(
                value: role,
                decoration: const InputDecoration(labelText: 'Uloga'),
                items: UserRole.values.map((r) => DropdownMenuItem(
                  value: r,
                  child: Text(r.name),
                )).toList(),
                onChanged: widget.forceRoleEmployee ? null : (v) => setState(() { if (v != null) role = v; }),
                disabledHint: const Text('Employee'),
              ),
              SwitchListTile(
                title: const Text('Aktivan'),
                value: isActive,
                onChanged: (v) => setState(() => isActive = v),
              ),
              if (error != null)
                Padding(
                  padding: const EdgeInsets.only(top: 8.0),
                  child: Text(error!, style: const TextStyle(color: Colors.red)),
                ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(onPressed: () => Navigator.pop(context), child: const Text('Otkaži')),
        ElevatedButton(
          onPressed: isLoading ? null : _submit,
          child: isLoading ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2)) : Text(widget.employee == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}