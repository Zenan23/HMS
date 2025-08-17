import 'package:flutter/material.dart';
import '../models/hotel.dart';
import '../services/api_service.dart';
import '../utils/validation_utils.dart';
import 'dart:convert';

class HotelFormDialog extends StatefulWidget {
  final Hotel? hotel;
  const HotelFormDialog({super.key, this.hotel});

  @override
  State<HotelFormDialog> createState() => _HotelFormDialogState();
}

class _HotelFormDialogState extends State<HotelFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late int id;
  late String name;
  late String address;
  late String city;
  late String country;
  late String phoneNumber;
  late String email;
  late String description;
  late int starRating;
  late String imageUrl;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final h = widget.hotel;
    id = h?.id ?? 0;
    name = h?.name ?? '';
    address = h?.address ?? '';
    city = h?.city ?? '';
    country = h?.country ?? '';
    phoneNumber = h?.phoneNumber ?? '';
    email = h?.email ?? '';
    description = h?.description ?? '';
    starRating = h?.starRating ?? 1;
    imageUrl = h?.imageUrl ?? '';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': id,
      'name': name,
      'address': address,
      'city': city,
      'country': country,
      'phoneNumber': phoneNumber,
      'email': email,
      'description': description,
      'starRating': starRating,
      'imageUrl': imageUrl,
    };
    try {
      if (widget.hotel == null) {
        // Dodavanje
        await ApiService().post('/api/hotels', body);
      } else {
        // Uređivanje
        await ApiService().put('/api/hotels/${widget.hotel!.id}', body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() {
        error = e.toString();
      });
    }
    setState(() {
      isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.hotel == null ? 'Dodaj hotel' : 'Uredi hotel'),
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
                initialValue: address,
                decoration: const InputDecoration(labelText: 'Adresa'),
                onChanged: (v) => address = v,
                validator: ValidationUtils.validateAddress,
              ),
              TextFormField(
                initialValue: city,
                decoration: const InputDecoration(labelText: 'Grad'),
                onChanged: (v) => city = v,
                validator: ValidationUtils.validateCity,
              ),
              TextFormField(
                initialValue: country,
                decoration: const InputDecoration(labelText: 'Država'),
                onChanged: (v) => country = v,
                validator: ValidationUtils.validateCity,
              ),
              TextFormField(
                initialValue: phoneNumber,
                decoration: const InputDecoration(labelText: 'Telefon'),
                onChanged: (v) => phoneNumber = v,
                validator: ValidationUtils.validatePhoneNumber,
              ),
              TextFormField(
                initialValue: email,
                decoration: const InputDecoration(labelText: 'Email'),
                onChanged: (v) => email = v,
                validator: ValidationUtils.validateEmail,
              ),
              TextFormField(
                initialValue: description,
                decoration: const InputDecoration(labelText: 'Opis'),
                maxLines: 3,
                onChanged: (v) => description = v,
                validator: ValidationUtils.validateDescription,
              ),
              TextFormField(
                initialValue: imageUrl,
                decoration: const InputDecoration(labelText: 'URL slike (opcionalno)'),
                onChanged: (v) => imageUrl = v,
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
              : Text(widget.hotel == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}
