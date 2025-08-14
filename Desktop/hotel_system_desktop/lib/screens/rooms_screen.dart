import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/room.dart';
import '../services/api_service.dart';
import '../widgets/room_form.dart';

class RoomsScreen extends StatefulWidget {
  const RoomsScreen({super.key});

  @override
  State<RoomsScreen> createState() => _RoomsScreenState();
}

class _RoomsScreenState extends State<RoomsScreen> {
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  List<Room> _rooms = [];

  @override
  void initState() {
    super.initState();
    _fetchRooms(_page);
  }

  Future<void> _fetchRooms(int page) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService()
          .get('/api/Rooms?pageNumber=$page&pageSize=$_pageSize');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final rooms = items.map((e) => Room.fromJson(e)).toList();
      setState(() {
        _rooms = rooms;
        _page = page;
        int totalCount = data['totalCount'] ?? 0;
        _totalPages = (totalCount / _pageSize).ceil();
      });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Greška: $e')));
      }
    }
    setState(() => _isLoading = false);
  }

  void _openRoomForm({Room? room}) async {
    final result = await showDialog(
      context: context,
      builder: (context) => RoomFormDialog(room: room),
    );
    if (result == true) {
      _fetchRooms(_page);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Sobe')),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                ElevatedButton.icon(
                  icon: const Icon(Icons.add),
                  label: const Text('Dodaj sobu'),
                  onPressed: _openRoomForm,
                ),
              ],
            ),
            const SizedBox(height: 16),
            Expanded(
              child: SingleChildScrollView(
    scrollDirection: Axis.vertical,
    child: SingleChildScrollView(
      scrollDirection: Axis.horizontal,
                child: DataTable(
                  columns: const [
                    DataColumn(label: Text('Broj')),
                    DataColumn(label: Text('Tip')),
                    DataColumn(label: Text('Cijena/noć')),
                    DataColumn(label: Text('Max. osoba')),
                    DataColumn(label: Text('Opis')),
                    DataColumn(label: Text('Dostupna')),
                    DataColumn(label: Text('Hotel')),
                    DataColumn(label: Text('Kreiran')),
                    DataColumn(label: Text('Ažuriran')),
                    DataColumn(label: Text('Uredi')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: _rooms
                      .map(
                        (r) => DataRow(
                          cells: [
                            DataCell(Text(r.roomNumber)),
                            DataCell(Text(r.roomType.name)),
                            DataCell(Text(r.pricePerNight.toStringAsFixed(2))),
                            DataCell(Text(r.maxOccupancy.toString())),
                            DataCell(Text(r.description)),
                            DataCell(Icon(
                                r.isAvailable ? Icons.check : Icons.close,
                                color:
                                    r.isAvailable ? Colors.green : Colors.red)),
                            DataCell(Text(r.hotelName ?? r.hotelId.toString())),
                            DataCell(Text(r.createdAt.toString())),
                            DataCell(Text(r.updatedAt.toString())),
                            DataCell(
                              IconButton(
                                icon: const Icon(Icons.edit),
                                tooltip: 'Uredi',
                                onPressed: () => _openRoomForm(room: r),
                              ),
                            ),
                            DataCell(
                              IconButton(
                                icon:
                                    const Icon(Icons.delete, color: Colors.red),
                                tooltip: 'Obriši',
                                onPressed: () async {
                                  final confirm = await showDialog<bool>(
                                    context: context,
                                    builder: (context) => AlertDialog(
                                      title: const Text('Potvrda brisanja'),
                                      content: Text(
                                          'Obrisati sobu ${r.roomNumber}?'),
                                      actions: [
                                        TextButton(
                                            onPressed: () =>
                                                Navigator.pop(context, false),
                                            child: const Text('Ne')),
                                        ElevatedButton(
                                            onPressed: () =>
                                                Navigator.pop(context, true),
                                            child: const Text('Da')),
                                      ],
                                    ),
                                  );
                                  if (confirm == true) {
                                    try {
                                      await ApiService()
                                          .delete('/api/Rooms/${r.id}');
                                      if (mounted) _fetchRooms(_page);
                                    } catch (e) {
                                      if (mounted) {
                                        ScaffoldMessenger.of(context)
                                            .showSnackBar(SnackBar(
                                                content: Text('Greška: $e')));
                                      }
                                    }
                                  }
                                },
                              ),
                            ),
                          ],
                        ),
                      )
                      .toList(),
                ),
              ),
              ),
            ),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: _page > 1 && !_isLoading
                      ? () => _fetchRooms(_page - 1)
                      : null,
                  child: const Text('Prethodna'),
                ),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                  child: Text('Stranica $_page / $_totalPages'),
                ),
                ElevatedButton(
                  onPressed: _page < _totalPages && !_isLoading
                      ? () => _fetchRooms(_page + 1)
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
      ),
    );
  }
}
