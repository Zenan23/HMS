import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:hotel_system_desktop/models/service.dart';
import '../models/hotel.dart';
import '../services/api_service.dart';
import '../utils/validation_utils.dart';

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
  late String category;
  late bool isAvailable;
  late bool isActive;
  late int? hotelId;

  bool isLoading = false;
  String? error;
  List<Hotel> _hotels = [];

  @override
  void initState() {
    super.initState();
    _fetchHotels();
    final s = widget.service;
    id = s?.id ?? 0;
    name = s?.name ?? '';
    description = s?.description ?? '';
    price = s?.price ?? 0;
    category = s?.category ?? '';
    isAvailable = s?.isAvailable ?? false;
    isActive = s?.isActive ?? false;
    hotelId = s?.hotelId;
  }

  Future<void> _fetchHotels() async {
    try {
      final response =
          await ApiService().get('/api/Hotels?pageNumber=1&pageSize=100');
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

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (hotelId == null) {
      setState(() => error = 'Odaberite hotel');
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
      'category': category,
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
      setState(() => error = e.toString());
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.service == null ? 'Dodaj servis' : 'Uredi servis'),
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
                keyboardType:
                    const TextInputType.numberWithOptions(decimal: true),
                onChanged: (v) => price = double.tryParse(v) ?? 0,
                validator: ValidationUtils.validatePrice,
              ),
              TextFormField(
                initialValue: category,
                decoration: const InputDecoration(labelText: 'Kategorija'),
                onChanged: (v) => category = v,
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
                    .map((h) =>
                        DropdownMenuItem<int>(value: h.id, child: Text(h.name)))
                    .toList(),
                onChanged: (v) => setState(() => hotelId = v),
                validator: (v) => v == null ? 'Obavezno' : null,
              ),
              if (error != null)
                Padding(
                  padding: const EdgeInsets.only(top: 8.0),
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
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2))
              : Text(widget.service == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}
