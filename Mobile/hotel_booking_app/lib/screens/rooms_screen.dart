import 'package:flutter/material.dart';
import '../models/hotel.dart';
import '../services/hotels_service.dart';

class RoomsScreen extends StatefulWidget {
  const RoomsScreen({Key? key}) : super(key: key);

  @override
  State<RoomsScreen> createState() => _RoomsScreenState();
}

class _RoomsScreenState extends State<RoomsScreen> {
  int _page = 1;
  final int _pageSize = 10;
  bool _loading = false;
  String? _error;
  List<Hotel> _hotels = [];

  @override
  void initState() {
    super.initState();
    _fetchHotels();
  }

  Future<void> _fetchHotels() async {
    setState(() { _loading = true; _error = null; });
    try {
      final hotels = await HotelsService().fetchHotels(page: _page, pageSize: _pageSize);
      setState(() { _hotels = hotels; });
    } catch (e) {
      setState(() { _error = 'Greška pri dohvatu hotela'; });
    }
    setState(() { _loading = false; });
  }

  void _nextPage() {
    setState(() { _page++; });
    _fetchHotels();
  }

  void _prevPage() {
    if (_page > 1) {
      setState(() { _page--; });
      _fetchHotels();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Hoteli')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Text(_error!))
              : Column(
                  children: [
                    Expanded(
                      child: ListView.builder(
                        itemCount: _hotels.length,
                        itemBuilder: (context, index) {
                          final hotel = _hotels[index];
                          return Card(
                            margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                            child: ListTile(
                              title: Text(hotel.name),
                              subtitle: Text('${hotel.city}, ${hotel.country}\n${hotel.description}'),
                              trailing: Row(
                                mainAxisSize: MainAxisSize.min,
                                children: [
                                  _buildStarRatingRow(hotel.averageRating ?? hotel.starRating.toDouble()),
                                ],
                              ),
                              onTap: () {
                                // TODO: Navigacija na detalje hotela/soba
                              },
                            ),
                          );
                        },
                      ),
                    ),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        IconButton(
                          icon: const Icon(Icons.arrow_back),
                          onPressed: _page > 1 ? _prevPage : null,
                        ),
                        Text('Stranica $_page'),
                        IconButton(
                          icon: const Icon(Icons.arrow_forward),
                          onPressed: _hotels.length == _pageSize ? _nextPage : null,
                        ),
                      ],
                    ),
                  ],
                ),
    );
  }
  Widget _buildStarRatingRow(double rating) {
    final stars = List<Widget>.generate(5, (index) {
      final starIndex = index + 1;
      IconData icon;
      if (rating >= starIndex) {
        icon = Icons.star;
      } else if (rating >= starIndex - 0.5) {
        icon = Icons.star_half;
      } else {
        icon = Icons.star_border;
      }
      return Icon(icon, color: Colors.amber, size: 18);
    });
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        ...stars,
        const SizedBox(width: 4),
        Text(
          rating.toStringAsFixed(1),
          style: const TextStyle(fontSize: 12, color: Colors.grey),
        ),
      ],
    );
  }
}