import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:hotel_system_desktop/models/room.dart';
import '../models/hotel.dart';
import '../services/api_service.dart';

class RoomFormDialog extends StatefulWidget {
  final Room? room;
  const RoomFormDialog({super.key, this.room});

  @override
  State<RoomFormDialog> createState() => _RoomFormDialogState();
}

class _RoomFormDialogState extends State<RoomFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late int id;
  late String roomNumber;
  late RoomType roomType;
  late double pricePerNight;
  late int maxOccupancy;
  late String description;
  late bool isAvailable;
  late int? hotelId;

  bool isLoading = false;
  String? error;
  List<Hotel> _hotels = [];

  @override
  void initState() {
    super.initState();
    _fetchHotels();
    final r = widget.room;
    id = r?.id ?? 0;
    roomNumber = r?.roomNumber ?? '';
    roomType = r?.roomType ?? RoomType.Single;
    pricePerNight = r?.pricePerNight ?? 0.01;
    maxOccupancy = r?.maxOccupancy ?? 1;
    description = r?.description ?? '';
    isAvailable = r?.isAvailable ?? true;
    hotelId = r?.hotelId;
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
      // ignore, dropdown can stay empty if error
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
      'roomNumber': roomNumber,
      'roomType': roomTypeToInt(roomType),
      'pricePerNight': pricePerNight,
      'maxOccupancy': maxOccupancy,
      'description': description,
      'isAvailable': isAvailable,
      'hotelId': hotelId,
    };
    try {
      if (widget.room == null) {
        await ApiService().post('/api/Rooms', body);
      } else {
        await ApiService().put('/api/Rooms/${widget.room!.id}', body);
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
      title: Text(widget.room == null ? 'Dodaj sobu' : 'Uredi sobu'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: roomNumber,
                decoration: const InputDecoration(labelText: 'Broj sobe'),
                onChanged: (v) => roomNumber = v,
                validator: (v) =>
                    v == null || v.isEmpty ? 'Obavezno polje' : null,
              ),
              DropdownButtonFormField<RoomType>(
                value: roomType,
                decoration: const InputDecoration(labelText: 'Tip sobe'),
                items: RoomType.values
                    .map((e) => DropdownMenuItem<RoomType>(
                          value: e,
                          child: Text(e.name),
                        ))
                    .toList(),
                onChanged: (v) => setState(() {
                  if (v != null) roomType = v;
                }),
                validator: (v) => v == null ? 'Obavezno' : null,
              ),
              TextFormField(
                initialValue: pricePerNight.toString(),
                decoration: const InputDecoration(labelText: 'Cijena po noći'),
                keyboardType:
                    const TextInputType.numberWithOptions(decimal: true),
                onChanged: (v) => pricePerNight = double.tryParse(v) ?? 0.01,
              ),
              TextFormField(
                initialValue: maxOccupancy.toString(),
                decoration: const InputDecoration(labelText: 'Max broj osoba'),
                keyboardType: TextInputType.number,
                onChanged: (v) => maxOccupancy = int.tryParse(v) ?? 1,
              ),
              TextFormField(
                initialValue: description,
                decoration: const InputDecoration(labelText: 'Opis'),
                onChanged: (v) => description = v,
              ),
              SwitchListTile(
                title: const Text('Dostupna'),
                value: isAvailable,
                onChanged: (v) => setState(() => isAvailable = v),
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
              : Text(widget.room == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}
