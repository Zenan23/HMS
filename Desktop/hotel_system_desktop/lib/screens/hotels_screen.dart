import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/hotel.dart';
import '../services/api_service.dart';
import '../widgets/hotel_form.dart';

class HotelsScreen extends StatefulWidget {
  const HotelsScreen({super.key});
  @override
  State<HotelsScreen> createState() => _HotelsScreenState();
}

class _HotelsScreenState extends State<HotelsScreen> {
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  List<Hotel> _hotels = [];
  String _searchQuery = '';
  bool _isSearchMode = false;

  @override
  void initState() {
    super.initState();
    _fetchHotels(_page);
  }

  Future<void> _fetchHotels(int page) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService()
          .get('/api/hotels?pageNumber=$page&pageSize=$_pageSize');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final hotels = items.map((e) => Hotel.fromJson(e)).toList();
      setState(() {
        _hotels = hotels;
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

  Future<void> _fetchHotelsByName(String name) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService().get('/api/hotels/by-city/$name');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final hotels = items.map((e) => Hotel.fromJson(e)).toList();
      
      setState(() {
        _hotels = hotels;
        _page = 1;
        _totalPages = 1;
        _isSearchMode = true;
      });
      
      if (hotels.isEmpty) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Hotel sa tim nazivom nije pronađen.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      setState(() {
        _hotels = [];
        _page = 1;
        _totalPages = 0;
        _isSearchMode = true;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Hotel sa tim nazivom nije pronađen.'),
          backgroundColor: Colors.orange,
        ),
      );
    }
    setState(() => _isLoading = false);
  }

  void _openHotelForm({Hotel? hotel}) async {
    final result = await showDialog(
      context: context,
      builder: (context) => HotelFormDialog(hotel: hotel),
    );
    if (result == true) {
      _fetchHotels(_page);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Hoteli')),
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
                      child: TextField(
                        decoration: const InputDecoration(
                          labelText: 'Pretraži po nazivu grada',
                          border: OutlineInputBorder(),
                          prefixIcon: Icon(Icons.search),
                        ),
                        onChanged: (value) {
                          setState(() {
                            _searchQuery = value;
                          });
                        },
                        onSubmitted: (value) {
                          if (value.isNotEmpty) {
                            _fetchHotelsByName(value);
                          }
                        },
                      ),
                    ),
                    const SizedBox(width: 8),
                    ElevatedButton(
                      onPressed: () {
                        if (_searchQuery.isNotEmpty) {
                          _fetchHotelsByName(_searchQuery);
                        }
                      },
                      child: const Text('Filtriraj'),
                    ),
                    const SizedBox(width: 8),
                    ElevatedButton(
                      onPressed: () {
                        setState(() {
                          _searchQuery = '';
                        });
                        _fetchHotels(1);
                      },
                      child: const Text('Očisti filtere'),
                    ),
                  ],
                ),
                ElevatedButton.icon(
                  icon: const Icon(Icons.add),
                  label: const Text('Dodaj hotel'),
                  onPressed: () => _openHotelForm(),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Expanded(
              child: _hotels.isEmpty && !_isLoading
                  ? Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            _isSearchMode ? Icons.search_off : Icons.hotel_outlined,
                            size: 64,
                            color: Colors.grey,
                          ),
                          const SizedBox(height: 16),
                          Text(
                            _isSearchMode 
                                ? 'Nema rezultata za pretragu'
                                : 'Nema hotela',
                            style: const TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                              color: Colors.grey,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Text(
                            _isSearchMode
                                ? 'Pokušajte sa drugim nazivom hotela'
                                : 'Dodajte prvi hotel',
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
                    DataColumn(label: Text('Slika')),
                    DataColumn(label: Text('Naziv')),
                    DataColumn(label: Text('Adresa')),
                    DataColumn(label: Text('Grad')),
                    DataColumn(label: Text('Država')),
                    DataColumn(label: Text('Telefon')),
                    DataColumn(label: Text('Email')),
                    DataColumn(label: Text('Opis')),
                    DataColumn(label: Text('Zvjezdice')),
                    DataColumn(label: Text('Kreiran')),
                    DataColumn(label: Text('Ažuriran')),
                    DataColumn(label: Text('Akcije')),
                  ],
                  rows: _hotels
                      .map((hotel) => DataRow(cells: [
                            DataCell(
                              hotel.imageUrl.isNotEmpty
                                  ? Image.network(
                                      hotel.imageUrl,
                                      width: 60,
                                      height: 40,
                                      fit: BoxFit.cover,
                                      errorBuilder: (_, __, ___) => const Icon(Icons.broken_image),
                                    )
                                  : const Icon(Icons.photo, size: 32),
                            ),
                            DataCell(Text(hotel.name)),
                            DataCell(Text(hotel.address)),
                            DataCell(Text(hotel.city)),
                            DataCell(Text(hotel.country)),
                            DataCell(Text(hotel.phoneNumber)),
                            DataCell(Text(hotel.email)),
                            DataCell(Text(hotel.description)),
                            DataCell(Text(hotel.averageRating.toString())),
                            DataCell(Text(hotel.createdAt.toString())),
                            DataCell(Text(hotel.updatedAt.toString())),
                            DataCell(Row(
                              children: [
                                IconButton(
                                  icon: const Icon(Icons.edit),
                                  tooltip: 'Uredi',
                                  onPressed: () => _openHotelForm(hotel: hotel),
                                ),
                                IconButton(
                                  icon: const Icon(Icons.delete),
                                  tooltip: 'Obriši',
                                  onPressed: () async {
                                    final confirm = await showDialog<bool>(
                                      context: context,
                                      builder: (context) => AlertDialog(
                                        title: const Text('Potvrda brisanja'),
                                        content: const Text(
                                            'Da li ste sigurni da želite obrisati hotel?'),
                                        actions: [
                                          TextButton(
                                            onPressed: () =>
                                                Navigator.pop(context, false),
                                            child: const Text('Ne'),
                                          ),
                                          TextButton(
                                            onPressed: () =>
                                                Navigator.pop(context, true),
                                            child: const Text('Da'),
                                          ),
                                        ],
                                      ),
                                    );
                                    if (confirm == true) {
                                      await ApiService()
                                          .delete('/api/hotels/${hotel.id}');
                                      _fetchHotels(_page);
                                    }
                                  },
                                ),
                              ],
                            )),
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
                      ? () => _fetchHotels(_page - 1)
                      : null,
                  child: const Text('Prethodna'),
                ),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                  child: Text('Stranica $_page / $_totalPages'),
                ),
                ElevatedButton(
                  onPressed: _page < _totalPages && !_isLoading
                      ? () => _fetchHotels(_page + 1)
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
