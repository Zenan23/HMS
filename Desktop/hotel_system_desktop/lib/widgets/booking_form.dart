import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/booking.dart';
import '../models/room.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../models/service.dart';

class BookingFormDialog extends StatefulWidget {
  final Booking? booking;
  const BookingFormDialog({super.key, this.booking});

  @override
  State<BookingFormDialog> createState() => _BookingFormDialogState();
}

class _BookingFormDialogState extends State<BookingFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late int id;
  late DateTime? checkInDate;
  late DateTime? checkOutDate;
  late int numberOfGuests;
  late String specialRequests;
  late double totalPrice;
  late int roomId;
  late int userId;

  BookingStatus status = BookingStatus.Pending;

  bool isLoading = false;
  String? error;
  List<Room> _rooms = [];
  List<Employee> _guests = [];
  List<Service> _services = [];
  final Map<int, int> _selectedServices = {}; // serviceId -> qty

  @override
  void initState() {
    super.initState();
    final b = widget.booking;
    id = b?.id ?? 0;
    checkInDate = b?.checkInDate;
    checkOutDate = b?.checkOutDate;
    numberOfGuests = b?.numberOfGuests ?? 1;
    specialRequests = b?.specialRequests ?? '';
    totalPrice = b?.totalPrice ?? 0.0;
    roomId = b?.roomId ?? 0;
    userId = b?.userId ?? 0;
    status = b?.status ?? BookingStatus.Pending;
    _fetchRoomsAndGuests();
    // Pre-populate selected services for edit
    if (b != null && b.services.isNotEmpty) {
      for (final item in b.services) {
        _selectedServices[item.serviceId] = item.quantity;
      }
    }
  }

  Future<void> _fetchRoomsAndGuests() async {
    try {
      // Fetch rooms
      final roomsResp =
          await ApiService().get('/api/Rooms?pageNumber=1&pageSize=100');
      final roomsDecoded = jsonDecode(roomsResp.body) as Map<String, dynamic>;
      final roomsItems = (roomsDecoded['data']?['items'] as List?) ?? [];
      _rooms = roomsItems.map((e) => Room.fromJson(e)).toList().cast<Room>();
    } catch (_) {}
    try {
      // Fetch guests (role = Guest -> 0)
      final guestsResp =
          await ApiService().get('/api/Users/role/0?pageNumber=1&pageSize=100');
      final guestsDecoded = jsonDecode(guestsResp.body);
      final List guestItems = (guestsDecoded['data'] ?? []) as List;
      _guests =
          guestItems.map((e) => Employee.fromJson(e)).toList().cast<Employee>();
    } catch (_) {}
    if (roomId != 0 && _rooms.isNotEmpty) {
      await _loadServicesForSelectedRoom();
    }
    if (mounted) setState(() {});
  }

  Future<void> _loadServicesForSelectedRoom() async {
    try {
      final room = _rooms.firstWhere((r) => r.id == roomId, orElse: () => _rooms.first);
      final hotelId = room.hotelId;
      final resp = await ApiService().get('/api/Services/hotel/$hotelId');
      final decoded = jsonDecode(resp.body) as Map<String, dynamic>;
      final List list = decoded['data'] ?? [];
      _services = list.map((e) => Service.fromJson(e)).toList();
      _selectedServices.removeWhere((sid, _) => !_services.any((s) => s.id == sid));
      if (mounted) setState(() {});
    } catch (_) {
      _services = [];
      if (mounted) setState(() {});
    }
  }

  Future<void> _pickDate({required bool isCheckIn}) async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: (isCheckIn ? checkInDate : checkOutDate) ?? now,
      firstDate: DateTime(now.year - 1),
      lastDate: DateTime(now.year + 5),
    );
    if (picked != null) {
      setState(() {
        if (isCheckIn) {
          checkInDate = picked;
        } else {
          checkOutDate = picked;
        }
      });
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (checkInDate == null || checkOutDate == null) {
      setState(() => error = 'Odaberite datume check-in i check-out');
      return;
    }
    if (checkInDate!.isAfter(checkOutDate!)) {
      setState(() => error = 'Check-out mora biti nakon check-ina');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': id,
      'checkIn': checkInDate!.toIso8601String(),
      'checkOut': checkOutDate!.toIso8601String(),
      'numberOfGuests': numberOfGuests,
      'specialRequests': specialRequests,
      'totalPrice': totalPrice,
      'roomId': roomId,
      'userId': userId,
      'status': bookingStatusToInt(status),
      'services': _selectedServices.entries
          .map((e) => {'serviceId': e.key, 'quantity': e.value})
          .toList(),
    };
    // Debug: provjeri da payload sadrži novi status
    // ignore: avoid_print
    try {
      if (widget.booking == null) {
        await ApiService().post('/api/Bookings', body);
      } else {
        await ApiService().put('/api/Bookings/${widget.booking!.id}', body);
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
      title: Text(
          widget.booking == null ? 'Dodaj rezervaciju' : 'Uredi rezervaciju'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      readOnly: true,
                      decoration:
                          const InputDecoration(labelText: 'Check-in datum'),
                      controller: TextEditingController(
                          text: checkInDate?.toString().split(' ').first ?? ''),
                      onTap: () => _pickDate(isCheckIn: true),
                      validator: (_) => checkInDate == null ? 'Obavezno' : null,
                    ),
                  ),
                  const SizedBox(width: 8),
                  Expanded(
                    child: TextFormField(
                      readOnly: true,
                      decoration:
                          const InputDecoration(labelText: 'Check-out datum'),
                      controller: TextEditingController(
                          text:
                              checkOutDate?.toString().split(' ').first ?? ''),
                      onTap: () => _pickDate(isCheckIn: false),
                      validator: (_) =>
                          checkOutDate == null ? 'Obavezno' : null,
                    ),
                  ),
                ],
              ),
              TextFormField(
                decoration: const InputDecoration(labelText: 'Broj gostiju'),
                keyboardType: TextInputType.number,
                initialValue: numberOfGuests.toString(),
                onChanged: (v) => numberOfGuests = int.tryParse(v) ?? 1,
              ),
              TextFormField(
                decoration:
                    const InputDecoration(labelText: 'Posebni zahtjevi'),
                initialValue: specialRequests,
                onChanged: (v) => specialRequests = v,
              ),
              TextFormField(
                decoration: const InputDecoration(labelText: 'Ukupna cijena'),
                keyboardType:
                    const TextInputType.numberWithOptions(decimal: true),
                initialValue: totalPrice.toString(),
                onChanged: (v) => totalPrice = double.tryParse(v) ?? 0,
              ),
              DropdownButtonFormField<int>(
                value: roomId == 0 ? null : roomId,
                decoration: const InputDecoration(labelText: 'Soba'),
                items: _rooms
                    .map((r) => DropdownMenuItem<int>(
                          value: r.id,
                          child: Text(
                              '${r.roomNumber} - ${r.hotelName ?? 'Hotel ' + r.hotelId.toString()}'),
                        ))
                    .toList(),
                onChanged: (v) async {
                  setState(() => roomId = v ?? 0);
                  await _loadServicesForSelectedRoom();
                },
                validator: (v) => (v == null || v == 0) ? 'Obavezno' : null,
              ),
              const SizedBox(height: 8),
              if (_services.isNotEmpty) ...[
                Align(
                  alignment: Alignment.centerLeft,
                  child: Text('Dodatne usluge', style: Theme.of(context).textTheme.bodyMedium),
                ),
                const SizedBox(height: 6),
                ..._services.map((svc) => CheckboxListTile(
                      contentPadding: EdgeInsets.zero,
                      value: _selectedServices.containsKey(svc.id),
                      onChanged: (v) {
                        setState(() {
                          if (v == true) {
                            _selectedServices[svc.id] = 1;
                          } else {
                            _selectedServices.remove(svc.id);
                          }
                        });
                      },
                      title: Text('${svc.name} (+${svc.price.toStringAsFixed(2)} €)'),
                      subtitle: _selectedServices.containsKey(svc.id)
                          ? Row(children: [
                              const Text('Količina:'),
                              IconButton(icon: const Icon(Icons.remove), onPressed: () {
                                setState(() {
                                  final q = (_selectedServices[svc.id] ?? 1);
                                  if (q > 1) _selectedServices[svc.id] = q - 1;
                                });
                              }),
                              Text('${_selectedServices[svc.id] ?? 1}'),
                              IconButton(icon: const Icon(Icons.add), onPressed: () {
                                setState(() {
                                  final q = (_selectedServices[svc.id] ?? 1);
                                  _selectedServices[svc.id] = q + 1;
                                });
                              }),
                            ])
                          : null,
                    )),
              ],
              DropdownButtonFormField<int>(
                value: userId == 0 ? null : userId,
                decoration: const InputDecoration(labelText: 'Gost'),
                items: _guests
                    .map((g) => DropdownMenuItem<int>(
                          value: g.id,
                          child: Text(
                              '${g.fullName.isNotEmpty ? g.fullName : g.username} (${g.email})'),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => userId = v ?? 0),
                validator: (v) => (v == null || v == 0) ? 'Obavezno' : null,
              ),
              DropdownButtonFormField<BookingStatus>(
                value: status,
                decoration: const InputDecoration(labelText: 'Status'),
                items: BookingStatus.values
                    .map((s) => DropdownMenuItem(value: s, child: Text(s.name)))
                    .toList(),
                onChanged: (v) => setState(() {
                  if (v != null) status = v;
                }),
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
              : Text(widget.booking == null ? 'Dodaj' : 'Spasi'),
        ),
      ],
    );
  }
}
