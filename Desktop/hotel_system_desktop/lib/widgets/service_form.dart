import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:hotel_system_desktop/models/service.dart';
import '../models/hotel.dart';
import '../services/api_service.dart';
import '../utils/validation_utils.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';

class ServiceFormDialog extends StatefulWidget {
  final Service? service;
  const ServiceFormDialog({super.key, this.service});

  @override
  State<ServiceFormDialog> createState() => _ServiceFormDialogState();
}

class _ServiceFormDialogState extends State<ServiceFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late int id;
  late String name;
  late String description;
  late double price;
  late int? serviceCategoryId;
  late bool isAvailable;
  late bool isActive;
  late int? hotelId;

  bool isLoading = false;
  String? error;
  List<Hotel> _hotels = [];

  // Kategorija je na backendu FK (ServiceCategoryId, obavezan) — ne slobodan
  // tekst. Ranije je ovdje bilo tekstualno polje koje je slalo 'category'
  // (string) koji CreateServiceDto/UpdateServiceDto uopšte nema, pa je
  // ServiceCategoryId ostajao 0 i backend je ODBIJAO SVAKI pokušaj dodavanja
  // servisa sa "Kategorija je obavezna." — otud je dodavanje servisa bilo
  // potpuno nemoguće, bez obzira na rolu.
  List<Map<String, dynamic>> _categories = [];
  bool _checkingCategories = true;

  @override
  void initState() {
    super.initState();
    _fetchHotels();
    _fetchServiceCategories();
    final s = widget.service;
    id = s?.id ?? 0;
    name = s?.name ?? '';
    description = s?.description ?? '';
    price = s?.price ?? 0;
    serviceCategoryId = (s != null && s.serviceCategoryId > 0)
        ? s.serviceCategoryId
        : null;
    isAvailable = s?.isAvailable ?? false;
    isActive = s?.isActive ?? false;
    hotelId = s?.hotelId;
  }

  Future<void> _fetchHotels() async {
    try {
      final response = await ApiService().get(
        '/api/Hotels?pageNumber=1&pageSize=100',
      );
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      setState(() {
        _hotels = items.map((e) => Hotel.fromJson(e)).toList().cast<Hotel>();
      });
    } catch (e) {
      // ignore
    }
  }

  Future<void> _fetchServiceCategories() async {
    try {
      final response = await ApiService().get(
        '/api/ServiceCategories?pageNumber=1&pageSize=200',
      );
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final categories = items
          .map((e) => {'id': e['id'], 'name': (e['name'] ?? '').toString()})
          .toList();
      if (mounted) setState(() => _categories = categories);
    } catch (_) {
      // ignore — dropdown ostaje prazan, validator će tražiti odabir
    }
    if (mounted) setState(() => _checkingCategories = false);
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (hotelId == null) {
      setState(() => error = 'Odaberite hotel');
      return;
    }
    if (serviceCategoryId == null) {
      setState(() => error = 'Odaberite kategoriju');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': id,
      'name': name,
      'description': description,
      'price': price,
      'serviceCategoryId': serviceCategoryId,
      'isAvailable': isAvailable,
      'isActive': isActive,
      'hotelId': hotelId,
    };
    try {
      if (widget.service == null) {
        await ApiService().post('/api/Services', body);
      } else {
        await ApiService().put('/api/Services/${widget.service!.id}', body);
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
      title: AppDialogTitle(
        widget.service == null ? 'Dodaj servis' : 'Uredi servis',
      ),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: name,
                decoration: const InputDecoration(labelText: 'Naziv'),
                onChanged: (v) => name = v,
                validator: ValidationUtils.validateHotelName,
              ),
              TextFormField(
                initialValue: description,
                decoration: const InputDecoration(labelText: 'Opis'),
                maxLines: 3,
                onChanged: (v) => description = v,
                validator: ValidationUtils.validateDescription,
              ),
              TextFormField(
                initialValue: price.toString(),
                decoration: const InputDecoration(labelText: 'Cijena'),
                keyboardType: const TextInputType.numberWithOptions(
                  decimal: true,
                ),
                onChanged: (v) => price = double.tryParse(v) ?? 0,
                validator: ValidationUtils.validatePrice,
              ),
              _checkingCategories
                  ? const Padding(
                      padding: EdgeInsets.symmetric(vertical: 8),
                      child: LinearProgressIndicator(),
                    )
                  : DropdownButtonFormField<int>(
                      value:
                          _categories.any((c) => c['id'] == serviceCategoryId)
                          ? serviceCategoryId
                          : null,
                      decoration: const InputDecoration(
                        labelText: 'Kategorija',
                      ),
                      items: _categories
                          .map(
                            (c) => DropdownMenuItem<int>(
                              value: c['id'] as int,
                              child: Text(c['name'] as String),
                            ),
                          )
                          .toList(),
                      onChanged: (v) => setState(() => serviceCategoryId = v),
                      validator: (v) => v == null ? 'Obavezno' : null,
                    ),
              if (!_checkingCategories && _categories.isEmpty)
                const Padding(
                  padding: EdgeInsets.only(top: 4, bottom: 4),
                  child: Text(
                    'Nema nijedne kategorije servisa u bazi kontaktirajte '
                    'administratora da doda barem jednu (ServiceCategories).',
                    style: TextStyle(fontSize: 12, color: Colors.orange),
                  ),
                ),
              SwitchListTile(
                title: const Text('Dostupno'),
                value: isAvailable,
                onChanged: (v) => setState(() => isAvailable = v),
              ),
              SwitchListTile(
                title: const Text('Aktivno'),
                value: isActive,
                onChanged: (v) => setState(() => isActive = v),
              ),
              DropdownButtonFormField<int>(
                value: hotelId,
                decoration: const InputDecoration(labelText: 'Hotel'),
                items: _hotels
                    .map(
                      (h) => DropdownMenuItem<int>(
                        value: h.id,
                        child: Text(h.name),
                      ),
                    )
                    .toList(),
                onChanged: (v) => setState(() => hotelId = v),
                validator: (v) => v == null ? 'Obavezno' : null,
              ),
              if (error != null)
                Padding(
                  padding: const EdgeInsets.only(top: 8.0),
                  child: Text(
                    error!,
                    style: const TextStyle(color: Colors.red),
                  ),
                ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Otkaži'),
        ),
        ElevatedButton(
          onPressed: isLoading ? null : _submit,
          child: isLoading
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : Text(widget.service == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}
