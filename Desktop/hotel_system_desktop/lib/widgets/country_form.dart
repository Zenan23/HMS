import 'package:flutter/material.dart';
import '../models/country.dart';
import '../services/country_service.dart';
import 'app_dialog_title.dart';

class CountryFormDialog extends StatefulWidget {
  final Country? country;
  const CountryFormDialog({super.key, this.country});

  @override
  State<CountryFormDialog> createState() => _CountryFormDialogState();
}

class _CountryFormDialogState extends State<CountryFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = CountryService();
  late String name;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    name = widget.country?.name ?? '';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {'id': widget.country?.id ?? 0, 'name': name};
    try {
      if (widget.country == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.country!.id, body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() => error = e.toString());
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(widget.country == null ? 'Nova država' : 'Uredi državu'),
      content: Form(
        key: _formKey,
        child: TextFormField(
          initialValue: name,
          autofocus: true,
          decoration: const InputDecoration(labelText: 'Naziv države'),
          onChanged: (v) => name = v,
          validator: (v) =>
              (v == null || v.trim().isEmpty) ? 'Naziv je obavezan.' : null,
        ),
      ),
      actions: [
        if (error != null)
          Padding(
            padding: const EdgeInsets.only(right: 8),
            child: Text(error!, style: const TextStyle(color: Colors.red)),
          ),
        TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Otkaži')),
        ElevatedButton(
          onPressed: isLoading ? null : _submit,
          child: isLoading
              ? const SizedBox(
                  width: 20, height: 20, child: CircularProgressIndicator())
              : const Text('Spasi'),
        ),
      ],
    );
  }
}
