import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/booking.dart';
import '../widgets/booking_form.dart';
import '../services/api_service.dart';

class BookingsScreen extends StatefulWidget {
  const BookingsScreen({super.key});
  @override
  State<BookingsScreen> createState() => _BookingsScreenState();
}

class _BookingsScreenState extends State<BookingsScreen> {
  List<Booking> _bookings = [];
  bool _isLoading = false;
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;

  @override
  void initState() {
    super.initState();
    _fetchBookings(_page);
  }

  Future<void> _fetchBookings(int page) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService()
          .get('/api/Bookings?pageNumber=$page&pageSize=$_pageSize');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final bookings = items.map((e) => Booking.fromJson(e)).toList();
      setState(() {
        _bookings = bookings;
        _page = page;
        int totalCount = data['totalCount'] ?? 0;
        _totalPages = (totalCount / _pageSize).ceil();
      });
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Greška: $e')));
    }
    setState(() => _isLoading = false);
  }

  void _openBookingForm({Booking? booking}) async {
    final result = await showDialog(
      context: context,
      builder: (context) => BookingFormDialog(booking: booking),
    );
    if (result == true) {
      _fetchBookings(_page);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              ElevatedButton.icon(
                icon: const Icon(Icons.add),
                label: const Text('Dodaj rezervaciju'),
                onPressed: () => _openBookingForm(),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
              scrollDirection: Axis.vertical,
              child: DataTable(
                columns: const [
                  DataColumn(label: Text('Check in')),
                  DataColumn(label: Text('Check out')),
                  DataColumn(label: Text('BrGostiju')),
                  DataColumn(label: Text('Zahtjevi')),
                  DataColumn(label: Text('Cijena')),
                  DataColumn(label: Text('Ažurirano')),
                  DataColumn(label: Text('Uredi')),
                  DataColumn(label: Text('Obriši')),
                ],
                rows: _bookings
                    .map((booking) => DataRow(cells: [
                          DataCell(Text(booking.checkInDate.toString())),
                          DataCell(Text(booking.checkOutDate.toString())),
                          DataCell(Text(booking.numberOfGuests.toString())),
                          DataCell(Text(booking.specialRequests)),
                          DataCell(Text(booking.totalPrice.toStringAsFixed(2))),
                          DataCell(Text(booking.updatedAt.toString())),
                          DataCell(
                            IconButton(
                              icon: const Icon(Icons.edit),
                              tooltip: 'Uredi',
                              onPressed: () =>
                                  _openBookingForm(booking: booking),
                            ),
                          ),
                          DataCell(
                            IconButton(
                              icon: const Icon(Icons.delete, color: Colors.red),
                              tooltip: 'Obriši',
                              onPressed: () async {
                                final confirm = await showDialog<bool>(
                                  context: context,
                                  builder: (context) => AlertDialog(
                                    title: const Text('Potvrda brisanja'),
                                    content: const Text('Obrisati rezervaciju?'),
                                    actions: [
                                      TextButton(onPressed: () => Navigator.pop(context, false), child: const Text('Ne')),
                                      ElevatedButton(onPressed: () => Navigator.pop(context, true), child: const Text('Da')),
                                    ],
                                  ),
                                );
                                if (confirm == true) {
                                  try {
                                    await ApiService().delete('/api/Bookings/${booking.id}');
                                    if (mounted) _fetchBookings(_page);
                                  } catch (e) {
                                    if (mounted) {
                                      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Greška: $e')));
                                    }
                                  }
                                }
                              },
                            ),
                          ),
                        ]))
                    .toList(),
              ),
            ),
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              ElevatedButton(
                onPressed: _page > 1 && !_isLoading
                    ? () => _fetchBookings(_page - 1)
                    : null,
                child: const Text('Prethodna'),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: Text('Stranica $_page / $_totalPages'),
              ),
              ElevatedButton(
                onPressed: _page < _totalPages && !_isLoading
                    ? () => _fetchBookings(_page + 1)
                    : null,
                child: const Text('Sljedeća'),
              ),
            ],
          ),
          if (_isLoading)
            const Padding(
              padding: EdgeInsets.all(16.0),
              child: Center(child: CircularProgressIndicator()),
            ),
        ],
      ),
    );
  }
}
