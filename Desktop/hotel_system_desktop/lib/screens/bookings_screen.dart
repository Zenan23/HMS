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
  DateTime? _startDate;
  DateTime? _endDate;
  bool _isSearchMode = false;

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
        _isSearchMode = false;
      });
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Greška: $e')));
    }
    setState(() => _isLoading = false);
  }

  Future<void> _fetchBookingsByDateRange(DateTime startDate, DateTime endDate) async {
    setState(() => _isLoading = true);
    try {
      final startDateStr = startDate.toIso8601String();
      final endDateStr = endDate.toIso8601String();
      final response = await ApiService()
          .get('/api/Bookings/date-range?startDate=$startDateStr&endDate=$endDateStr');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final List data = decoded['data'] ?? [];
      final bookings = data.map((e) => Booking.fromJson(e)).toList();
      
      setState(() {
        _bookings = bookings;
        _page = 1;
        _totalPages = 1;
        _isSearchMode = true;
      });
      
      if (bookings.isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Nema rezervacija u odabranom periodu.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      setState(() {
        _bookings = [];
        _page = 1;
        _totalPages = 0;
        _isSearchMode = true;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Greška pri učitavanju rezervacija.'),
          backgroundColor: Colors.red,
        ),
      );
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
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  ElevatedButton(
                    onPressed: () async {
                      final date = await showDatePicker(
                        context: context,
                        initialDate: _startDate ?? DateTime.now(),
                        firstDate: DateTime(2020),
                        lastDate: DateTime.now().add(const Duration(days: 365)),
                      );
                      if (date != null) {
                        setState(() {
                          _startDate = date;
                        });
                        if (_endDate != null) {
                          _fetchBookingsByDateRange(date, _endDate!);
                        }
                      }
                    },
                    child: Text(_startDate != null 
                      ? 'Od: ${_startDate!.day}/${_startDate!.month}/${_startDate!.year}'
                      : 'Od datuma'),
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton(
                    onPressed: () async {
                      final date = await showDatePicker(
                        context: context,
                        initialDate: _endDate ?? DateTime.now(),
                        firstDate: DateTime(2020),
                        lastDate: DateTime.now().add(const Duration(days: 365)),
                      );
                      if (date != null) {
                        setState(() {
                          _endDate = date;
                        });
                        if (_startDate != null) {
                          _fetchBookingsByDateRange(_startDate!, date);
                        }
                      }
                    },
                    child: Text(_endDate != null 
                      ? 'Do: ${_endDate!.day}/${_endDate!.month}/${_endDate!.year}'
                      : 'Do datuma'),
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton(
                    onPressed: () {
                      setState(() {
                        _startDate = null;
                        _endDate = null;
                      });
                      _fetchBookings(1);
                    },
                    child: const Text('Očisti filtere'),
                  ),
                ],
              ),
              ElevatedButton.icon(
                icon: const Icon(Icons.add),
                label: const Text('Dodaj rezervaciju'),
                onPressed: () => _openBookingForm(),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: _bookings.isEmpty && !_isLoading
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          _isSearchMode ? Icons.search_off : Icons.calendar_today_outlined,
                          size: 64,
                          color: Colors.grey,
                        ),
                        const SizedBox(height: 16),
                        Text(
                          _isSearchMode 
                              ? 'Nema rezultata za pretragu'
                              : 'Nema rezervacija',
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.grey,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          _isSearchMode
                              ? 'Pokušajte sa drugim periodom'
                              : 'Dodajte prvu rezervaciju',
                          style: const TextStyle(
                            color: Colors.grey,
                          ),
                        ),
                      ],
                    ),
                  )
                : SingleChildScrollView(
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
          if (!_isSearchMode)
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
