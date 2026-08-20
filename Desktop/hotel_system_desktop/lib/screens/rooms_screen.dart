import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/room.dart';
import '../services/api_service.dart';
import '../services/pdf_report_service.dart';
import '../utils/display_labels.dart';
import '../widgets/room_form.dart';
import '../widgets/app_dialog_title.dart';

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
  String? _selectedRoomType;
  bool _isSearchMode = false;

  // Soba mora biti vezana za hotel (obavezan FK) — dok ne postoji nijedan
  // hotel u bazi, forma za dodavanje sobe se ne smije moći otvoriti.
  bool _checkingHotels = true;
  bool _hasHotels = false;

  @override
  void initState() {
    super.initState();
    _fetchRooms(_page);
    _checkHotelsExist();
  }

  Future<void> _checkHotelsExist() async {
    try {
      final response =
          await ApiService().get('/api/Hotels?pageNumber=1&pageSize=1');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final int totalCount = data['totalCount'] ?? 0;
      if (mounted) setState(() => _hasHotels = totalCount > 0);
    } catch (_) {
      // ako provjera padne, ne blokiramo korisnika zbog mrežne greške
      if (mounted) setState(() => _hasHotels = true);
    }
    if (mounted) setState(() => _checkingHotels = false);
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
        _isSearchMode = false;
      });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Greška: $e')));
      }
    }
    setState(() => _isLoading = false);
  }

  // Izvještaj mora obuhvatiti cijeli dataset, ne samo trenutno prikazanu
  // stranicu — ako je aktivan filter po tipu sobe, taj poziv već vraća sve
  // rezultate; u suprotnom dohvati sve stranice.
  Future<List<Room>> _fetchAllRoomsForExport() async {
    if (_isSearchMode) return _rooms;
    final List<Room> all = [];
    int page = 1;
    const int size = 100;
    while (true) {
      final response = await ApiService()
          .get('/api/Rooms?pageNumber=$page&pageSize=$size');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      all.addAll(items.map((e) => Room.fromJson(e)));
      final int totalCount = data['totalCount'] ?? 0;
      if (all.length >= totalCount || items.isEmpty) break;
      page++;
    }
    return all;
  }

  Future<void> _exportRoomsPdf() async {
    setState(() => _isLoading = true);
    try {
      final all = await _fetchAllRoomsForExport();
      if (mounted) PdfReportService.exportRooms(context, all);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Greška: $e')));
      }
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _fetchRoomsByType(String roomType) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService().get('/api/Rooms/by-type/$roomType');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final List data = decoded['data'] ?? [];
      final rooms = data.map((e) => Room.fromJson(e)).toList();
      
      setState(() {
        _rooms = rooms;
        _page = 1;
        _totalPages = 1;
        _isSearchMode = true;
      });
      
      if (rooms.isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Nema soba sa tim tipom.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      setState(() {
        _rooms = [];
        _page = 1;
        _totalPages = 0;
        _isSearchMode = true;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Greška pri učitavanju soba.'),
          backgroundColor: Colors.red,
        ),
      );
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
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  children: [
                    SizedBox(
                      width: 200,
                      child: DropdownButtonFormField<String>(
                        decoration: const InputDecoration(
                          labelText: 'Tip sobe',
                          border: OutlineInputBorder(),
                        ),
                        value: _selectedRoomType,
                        items: [
                          const DropdownMenuItem(
                            value: null,
                            child: Text('Svi tipovi'),
                          ),
                          ...RoomType.values.map((roomType) => DropdownMenuItem(
                            value: roomType.name,
                            child: Text(roomTypeLabel(roomType)),
                          )),
                        ],
                        onChanged: (value) {
                          setState(() {
                            _selectedRoomType = value;
                          });
                          if (value != null) {
                            _fetchRoomsByType(value);
                          } else {
                            _fetchRooms(1);
                          }
                        },
                      ),
                    ),
                    const SizedBox(width: 8),
                    ElevatedButton(
                      onPressed: () {
                        setState(() {
                          _selectedRoomType = null;
                        });
                        _fetchRooms(1);
                      },
                      child: const Text('Očisti filtere'),
                    ),
                  ],
                ),
                Row(
                  children: [
                    ElevatedButton.icon(
                      icon: const Icon(Icons.picture_as_pdf),
                      label: const Text('PDF'),
                      onPressed: _exportRoomsPdf,
                    ),
                    const SizedBox(width: 8),
                    Tooltip(
                      message: _checkingHotels || _hasHotels
                          ? ''
                          : 'Prvo dodajte barem jedan hotel — soba mora biti vezana za hotel.',
                      child: ElevatedButton.icon(
                        icon: const Icon(Icons.add),
                        label: const Text('Dodaj sobu'),
                        onPressed: !_checkingHotels && _hasHotels
                            ? _openRoomForm
                            : null,
                      ),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 16),
            Expanded(
              child: _rooms.isEmpty && !_isLoading
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            _isSearchMode ? Icons.search_off : Icons.bed_outlined,
                            size: 64,
                            color: Colors.grey,
                          ),
                          const SizedBox(height: 16),
                          Text(
                            _isSearchMode 
                                ? 'Nema rezultata za pretragu'
                                : 'Nema soba',
                            style: const TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                              color: Colors.grey,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            _isSearchMode
                                ? 'Pokušajte sa drugim tipom sobe'
                                : 'Dodajte prvu sobu',
                            style: const TextStyle(
                              color: Colors.grey,
                            ),
                          ),
                        ],
                      ),
                    )
                  : SingleChildScrollView(
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
                    DataColumn(label: Text('Uredi')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: _rooms
                      .map(
                        (r) => DataRow(
                          cells: [
                            DataCell(Text(r.roomNumber)),
                            DataCell(Text(roomTypeLabel(r.roomType))),
                            DataCell(Text(r.pricePerNight.toStringAsFixed(2))),
                            DataCell(Text(r.maxOccupancy.toString())),
                            DataCell(Text(r.description)),
                            DataCell(Icon(
                                r.isAvailable ? Icons.check : Icons.close,
                                color:
                                    r.isAvailable ? Colors.green : Colors.red)),
                            DataCell(Text(r.hotelName ?? r.hotelId.toString())),
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
                                      title: const AppDialogTitle('Potvrda brisanja'),
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
            if (!_isSearchMode)
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
