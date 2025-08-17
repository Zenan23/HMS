import 'package:flutter/material.dart';
import 'dart:convert';
import 'package:intl/intl.dart';
import '../services/rooms_service.dart';
import 'payment_screen.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';
import '../services/reservations_service.dart';
import '../services/api_service.dart';
import '../utils/validation_utils.dart';

class RoomBookingScreen extends StatefulWidget {
  final int roomId;
  final int maxOccupancy;
  const RoomBookingScreen(
      {Key? key, required this.roomId, required this.maxOccupancy})
      : super(key: key);

  @override
  State<RoomBookingScreen> createState() => _RoomBookingScreenState();
}

class _RoomBookingScreenState extends State<RoomBookingScreen> {
  DateTime? _checkIn;
  DateTime? _checkOut;
  int _guests = 1;
  bool _loading = false;
  String? _error;
  bool? _available;
  double? _price;
  // services selection: serviceId -> quantity
  final Map<int, int> _selectedServices = {};
  // available services for hotel (id, name, price)
  List<Map<String, dynamic>> _services = [];

  Future<void> _pickDate({required bool isCheckIn}) async {
    final now = DateTime.now();
    final picked = await showDatePicker(
      context: context,
      initialDate: now,
      firstDate: now,
      lastDate: now.add(const Duration(days: 365)),
    );
    if (picked != null) {
      setState(() {
        if (isCheckIn) {
          _checkIn = picked;
          if (_checkOut != null && _checkOut!.isBefore(_checkIn!)) {
            _checkOut = null;
          }
        } else {
          _checkOut = picked;
        }
      });
    }
  }

  Future<void> _checkAvailabilityAndPrice() async {
    if (_checkIn == null || _checkOut == null || _guests < 1) {
      setState(() {
        _error = 'Unesite sve podatke.';
      });
      return;
    }
    if (_guests > widget.maxOccupancy) {
      setState(() {
        _error = 'Broj gostiju ne smije biti veći od ${widget.maxOccupancy}.';
      });
      return;
    }
    setState(() {
      _loading = true;
      _error = null;
      _available = null;
      _price = null;
    });
    final checkInStr = DateFormat('yyyy-MM-dd').format(_checkIn!);
    final checkOutStr = DateFormat('yyyy-MM-dd').format(_checkOut!);
    final servicesStr = _buildServicesQuery();
    try {
      final available = await RoomsService().checkAvailability(
          widget.roomId, checkInStr, checkOutStr,
          services: servicesStr);
      if (!available) {
        setState(() {
          _available = false;
          _loading = false;
          _error = 'Soba nije dostupna u tom terminu.';
        });
        return;
      }
      final price = await RoomsService().calculatePrice(
          widget.roomId, checkInStr, checkOutStr, _guests,
          services: servicesStr);
      setState(() {
        _available = true;
        _price = price;
        _loading = false;
      });
    } catch (e) {
      print(e);
      setState(() {
        _error = 'Greška pri provjeri dostupnosti ili cijene.';
        _loading = false;
      });
    }
  }

  @override
  void initState() {
    super.initState();
    // Pokušaj učitati hotelId za sobu i zatim usluge tog hotela
    _initLoadServices();
  }

  Future<void> _initLoadServices() async {
    try {
      final resp = await ApiService.get('/Rooms/${widget.roomId}');
      if (resp.statusCode == 200) {
        final decoded = jsonDecode(resp.body) as Map<String, dynamic>;
        final data = decoded['data'] as Map<String, dynamic>?;
        final hotelId = data?['hotelId'];
        if (hotelId is int) {
          await _loadServicesForHotel(hotelId);
        }
      }
    } catch (_) {}
  }

  String _buildServicesQuery() {
    if (_selectedServices.isEmpty) return '';
    // format: id:qty,id2:qty2 (qty default 1)
    return _selectedServices.entries
        .map((e) => e.value > 1 ? '${e.key}:${e.value}' : '${e.key}')
        .join(',');
  }

  Future<void> _loadServicesForHotel(int hotelId) async {
    try {
      final resp = await ApiService.get('/Services/hotel/$hotelId');
      if (resp.statusCode == 200) {
        final decoded = jsonDecode(resp.body) as Map<String, dynamic>;
        final List list = decoded['data'] ?? [];
        _services = list
            .map((e) => {
                  'id': e['id'],
                  'name': e['name'],
                  'price': (e['price'] as num).toDouble(),
                })
            .toList();
        setState(() {});
      }
    } catch (_) {}
  }

  void _goToPayment() async {
    if (_price != null) {
      showDialog(
        context: context,
        barrierDismissible: false,
        builder: (context) => const Center(child: CircularProgressIndicator()),
      );
      try {
        final userId =
            Provider.of<AuthService>(context, listen: false).user?.userId;
        if (userId == null) {
          Navigator.pop(context); // zatvori loading
          setState(() {
            _error = 'Niste prijavljeni.';
          });
          return;
        }
        final servicesPayload = _selectedServices.entries
            .map((e) => {"serviceId": e.key, "quantity": e.value})
            .toList();

        final bookingData = {
          'userId': userId,
          'roomId': widget.roomId,
          'checkIn': _checkIn!.toIso8601String(),
          'checkOut': _checkOut!.toIso8601String(),
          'guests': _guests,
          'price': _price,
          'services': servicesPayload,
        };
        final bookingId =
            await ReservationsService().createBooking(bookingData);
        Navigator.pop(context); // zatvori loading
        if (bookingId == 0) {
          setState(() {
            _error = 'Greška pri kreiranju rezervacije.';
          });
          return;
        }
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => PaymentScreen(
              bookingId: bookingId,
              amount: _price!,
              currency: 'EUR',
            ),
          ),
        );
      } catch (e) {
        Navigator.pop(context); // zatvori loading
        setState(() {
          _error = 'Greška pri kreiranju rezervacije.';
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Rezervacija sobe')),
      body: SafeArea(
        child: LayoutBuilder(builder: (context, constraints) {
          return SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text('Check-in datum'),
                Row(
                  children: [
                    Expanded(
                      child: Text(_checkIn == null
                          ? 'Odaberite datum'
                          : DateFormat('dd.MM.yyyy').format(_checkIn!)),
                    ),
                    IconButton(
                      icon: const Icon(Icons.calendar_today),
                      onPressed: () => _pickDate(isCheckIn: true),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                const Text('Check-out datum'),
                Row(
                  children: [
                    Expanded(
                      child: Text(_checkOut == null
                          ? 'Odaberite datum'
                          : DateFormat('dd.MM.yyyy').format(_checkOut!)),
                    ),
                    IconButton(
                      icon: const Icon(Icons.calendar_today),
                      onPressed: () => _pickDate(isCheckIn: false),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Text('Broj gostiju (max ${widget.maxOccupancy})'),
                Row(
                  children: [
                    IconButton(
                      icon: const Icon(Icons.remove),
                      onPressed:
                          _guests > 1 ? () => setState(() => _guests--) : null,
                    ),
                    Text('$_guests'),
                    IconButton(
                      icon: const Icon(Icons.add),
                      onPressed: _guests < widget.maxOccupancy
                          ? () => setState(() => _guests++)
                          : null,
                    ),
                  ],
                ),
                const SizedBox(height: 24),
                const Text('Dodatne usluge'),
                const SizedBox(height: 8),
                if (_services.isNotEmpty) ...[
                  ..._services.map((svc) => CheckboxListTile(
                        value: _selectedServices.containsKey(svc['id']),
                        onChanged: (v) {
                          setState(() {
                            if (v == true) {
                              _selectedServices[svc['id']] = 1;
                            } else {
                              _selectedServices.remove(svc['id']);
                            }
                          });
                        },
                        title: Text('${svc['name']} (+${svc['price']} €)'),
                        subtitle: _selectedServices.containsKey(svc['id'])
                            ? Row(children: [
                                const Text('Količina:'),
                                IconButton(
                                    icon: const Icon(Icons.remove),
                                    onPressed: () {
                                      setState(() {
                                        final q =
                                            (_selectedServices[svc['id']] ?? 1);
                                        if (q > 1)
                                          _selectedServices[svc['id']] = q - 1;
                                      });
                                    }),
                                Text('${_selectedServices[svc['id']] ?? 1}'),
                                IconButton(
                                    icon: const Icon(Icons.add),
                                    onPressed: () {
                                      setState(() {
                                        final q =
                                            (_selectedServices[svc['id']] ?? 1);
                                        _selectedServices[svc['id']] = q + 1;
                                      });
                                    }),
                              ])
                            : null,
                      ))
                ],
                if (_error != null) ...[
                  Text(_error!, style: const TextStyle(color: Colors.red)),
                  const SizedBox(height: 12),
                ],
                const SizedBox(height: 80),
              ],
            ),
          );
        }),
      ),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 8, 16, 16),
          child: _loading
              ? const SizedBox(
                  height: 48,
                  child: Center(child: CircularProgressIndicator()),
                )
              : (_available == true && _price != null)
                  ? Row(
                      children: [
                        Expanded(
                          child: Text(
                            'Cijena: ${_price!.toStringAsFixed(2)} EUR',
                            style: const TextStyle(
                                fontSize: 16, fontWeight: FontWeight.w600),
                          ),
                        ),
                        ElevatedButton(
                          onPressed: _goToPayment,
                          child: const Text('Nastavi'),
                        ),
                      ],
                    )
                  : SizedBox(
                      width: double.infinity,
                      height: 48,
                      child: ElevatedButton(
                        onPressed: _checkAvailabilityAndPrice,
                        child: const Text('Provjeri dostupnost i cijenu'),
                      ),
                    ),
        ),
      ),
    );
  }
}
