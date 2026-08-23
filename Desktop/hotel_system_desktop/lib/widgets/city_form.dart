import 'package:flutter/material.dart';
import '../models/city.dart';
import '../models/country.dart';
import '../services/city_service.dart';
import '../services/country_service.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';

class CityFormDialog extends StatefulWidget {
  final City? city;
  const CityFormDialog({super.key, this.city});

  @override
  State<CityFormDialog> createState() => _CityFormDialogState();
}

class _CityFormDialogState extends State<CityFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = CityService();
  final _countryService = CountryService();
  late String name;
  int? countryId;
  List<Country> _countries = [];
  bool _loadingCountries = true;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    name = widget.city?.name ?? '';
    countryId = widget.city != null && widget.city!.countryId > 0
        ? widget.city!.countryId
        : null;
    _fetchCountries();
  }

  Future<void> _fetchCountries() async {
    try {
      final countries = await _countryService.getAllForDropdown();
      if (mounted) {
        setState(() {
          _countries = countries;
          _loadingCountries = false;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _loadingCountries = false);
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (countryId == null) {
      setState(() => error = 'Odaberite državu.');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.city?.id ?? 0,
      'name': name,
      'countryId': countryId,
    };
    try {
      if (widget.city == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.city!.id, body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() => error = friendlyErrorMessage(e));
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(widget.city == null ? 'Novi grad' : 'Uredi grad'),
      content: SizedBox(
        width: 400,
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: name,
                decoration: const InputDecoration(labelText: 'Naziv grada'),
                onChanged: (v) => name = v,
                validator: (v) => (v == null || v.trim().isEmpty)
                    ? 'Naziv je obavezan.'
                    : null,
              ),
              const SizedBox(height: 8),
              _loadingCountries
                  ? const Padding(
                      padding: EdgeInsets.symmetric(vertical: 8),
                      child: LinearProgressIndicator(),
                    )
                  : DropdownButtonFormField<int>(
                      value: _countries.any((c) => c.id == countryId)
                          ? countryId
                          : null,
                      decoration: const InputDecoration(labelText: 'Država'),
                      items: _countries
                          .map((c) => DropdownMenuItem<int>(
                                value: c.id,
                                child: Text(c.name),
                              ))
                          .toList(),
                      onChanged: (v) => setState(() => countryId = v),
                      validator: (v) => v == null ? 'Odaberite državu.' : null,
                    ),
              if (error != null)
                Padding(
                  padding: const EdgeInsets.only(top: 8),
                  child:
                      Text(error!, style: const TextStyle(color: Colors.red)),
                ),
            ],
          ),
        ),
      ),
      actions: [
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
