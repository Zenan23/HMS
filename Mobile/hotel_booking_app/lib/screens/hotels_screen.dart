import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/hotel.dart';
import '../services/hotels_service.dart';
import '../services/auth_service.dart';
import 'hotel_detail_screen.dart';

class HotelsScreen extends StatefulWidget {
  const HotelsScreen({Key? key}) : super(key: key);

  @override
  State<HotelsScreen> createState() => _HotelsScreenState();
}

class _HotelsScreenState extends State<HotelsScreen> {
  final TextEditingController _cityController = TextEditingController();
  int? _selectedRating;
  bool _isFiltering = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadHotels();
      final auth = context.read<AuthService>();
      if (auth.user != null) {
        context.read<HotelsService>().fetchRecommended(auth.user!.userId);
      }
    });
  }

  Future<void> _loadHotels() async {
    final hotelsService = Provider.of<HotelsService>(context, listen: false);
    try {
      final hotels = await hotelsService.fetchHotels();
      hotelsService.hotels = hotels;
      hotelsService.notifyListeners();
      setState(() { _isFiltering = false; });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString())),
        );
      }
    }
  }

  Future<void> _filterByCity() async {
    final city = _cityController.text.trim();
    if (city.isEmpty) return;
    setState(() { _isFiltering = true; });
    final hotelsService = Provider.of<HotelsService>(context, listen: false);
    try {
      final hotels = await hotelsService.fetchHotelsByCity(city);
      hotelsService.hotels = hotels;
      hotelsService.notifyListeners();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString())),
        );
      }
    }
    setState(() { _isFiltering = false; });
  }

  Future<void> _filterByRating(int? rating) async {
    if (rating == null) return;
    setState(() { _isFiltering = true; _selectedRating = rating; });
    final hotelsService = Provider.of<HotelsService>(context, listen: false);
    try {
      final hotels = await hotelsService.fetchHotelsByRating(rating);
      hotelsService.hotels = hotels;
      hotelsService.notifyListeners();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(e.toString())),
        );
      }
    }
    setState(() { _isFiltering = false; });
  }

  void _resetFilters() {
    _cityController.clear();
    _selectedRating = null;
    _loadHotels();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Hoteli'),
        backgroundColor: Theme.of(context).colorScheme.inversePrimary,
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _cityController,
                    decoration: const InputDecoration(
                      labelText: 'Grad',
                      border: OutlineInputBorder(),
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                ElevatedButton(
                  onPressed: _isFiltering ? null : _filterByCity,
                  child: const Text('Filtriraj'),
                ),
                const SizedBox(width: 8),
                DropdownButton<int>(
                  value: _selectedRating,
                  hint: const Text('Rating'),
                  items: List.generate(6, (i) => DropdownMenuItem(
                    value: i,
                    child: Text(i == 0 ? 'Svi' : i.toString()),
                  )),
                  onChanged: _isFiltering ? null : (v) {
                    if (v == 0) {
                      _resetFilters();
                    } else {
                      _filterByRating(v);
                    }
                  },
                ),
                const SizedBox(width: 8),
                IconButton(
                  icon: const Icon(Icons.refresh),
                  onPressed: _isFiltering ? null : _resetFilters,
                  tooltip: 'Resetuj filtere',
                ),
              ],
            ),
          ),
          if (context.watch<AuthService>().user != null)
            _buildRecommendedSection(),
          Expanded(
            child: Consumer<HotelsService>(
              builder: (context, hotelsService, child) {
                if (hotelsService.isLoading || _isFiltering) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (hotelsService.hotels.isEmpty) {
                  return Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.hotel_outlined, size: 64, color: Colors.grey),
                        const SizedBox(height: 16),
                        const Text(
                          'Nema dostupnih hotela',
                          style: TextStyle(fontSize: 18, color: Colors.grey),
                        ),
                        const SizedBox(height: 8),
                        ElevatedButton(
                          onPressed: _loadHotels,
                          child: const Text('Osvježi'),
                        ),
                      ],
                    ),
                  );
                }
                return RefreshIndicator(
                  onRefresh: _loadHotels,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(8.0),
                    itemCount: hotelsService.hotels.length,
                    itemBuilder: (context, index) {
                      final hotel = hotelsService.hotels[index];
                      return _buildHotelCard(hotel);
                    },
                  ),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildRecommendedSection() {
    return Consumer<HotelsService>(
      builder: (context, hotelsService, child) {
        final list = hotelsService.recommended;
        if (list.isEmpty) return const SizedBox.shrink();
        return SizedBox(
          height: 170,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
                child: Text('Preporučeno za vas', style: Theme.of(context).textTheme.titleMedium),
              ),
              Flexible(
                child: ListView.separated(
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                  scrollDirection: Axis.horizontal,
                  itemCount: list.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 12),
                  itemBuilder: (context, i) {
                    final h = list[i];
                    return SizedBox(
                      width: 150,
                      child: _buildHotelCardCompact(h),
                    );
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildHotelCardCompact(Hotel hotel) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
      child: InkWell(
        borderRadius: BorderRadius.circular(10),
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => HotelDetailScreen(hotel: hotel),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.start,
            children: [
              if (hotel.imageUrl.isNotEmpty) ...[
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    hotel.imageUrl,
                    height: 70,
                    width: double.infinity,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => Container(
                      height: 70,
                      color: Colors.grey.shade200,
                      child: const Center(child: Icon(Icons.broken_image)),
                    ),
                  ),
                ),
                const SizedBox(height: 8),
              ],
              Expanded(
                child: Center (
                  child: Text(
                hotel.name,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(fontSize: 10, fontWeight: FontWeight.bold),
              ),
                )
              )
             // const SizedBox(height: 4),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildHotelCard(Hotel hotel) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8.0),
      elevation: 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => HotelDetailScreen(hotel: hotel),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              if (hotel.imageUrl.isNotEmpty) ...[
                ClipRRect(
                  borderRadius: BorderRadius.circular(8),
                  child: Image.network(
                    hotel.imageUrl,
                    height: 150,
                    width: double.infinity,
                    fit: BoxFit.cover,
                    errorBuilder: (_, __, ___) => Container(
                      height: 150,
                      color: Colors.grey.shade200,
                      child: const Center(child: Icon(Icons.broken_image)),
                    ),
                  ),
                ),
                const SizedBox(height: 8),
              ],
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Text(
                      hotel.name,
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  _buildStarRatingRow(hotel.averageRating ?? hotel.starRating.toDouble()),
                ],
              ),
              const SizedBox(height: 8),
              Row(
                children: [
                  const Icon(Icons.location_on, size: 16, color: Colors.grey),
                  const SizedBox(width: 4),
                  Expanded(
                    child: Text(
                      '${hotel.city}, ${hotel.country}',
                      style: const TextStyle(
                        fontSize: 14,
                        color: Colors.grey,
                      ),
                    ),
                  ),
                ],
              ),
              if (hotel.description.isNotEmpty) ...[
                const SizedBox(height: 8),
                Text(
                  hotel.description,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontSize: 14),
                ),
              ],
              const SizedBox(height: 8),
              Row(
                children: [
                  const Icon(Icons.phone, size: 16, color: Colors.grey),
                  const SizedBox(width: 4),
                  Text(
                    hotel.phoneNumber,
                    style: const TextStyle(
                      fontSize: 12,
                      color: Colors.grey,
                    ),
                  ),
                  const Spacer(),
                  const Icon(Icons.arrow_forward_ios, size: 16, color: Colors.grey),
                ],
              ),
            ],
          ),
        ),
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
      return Icon(icon, color: Colors.amber, size: 20);
    });
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        ...stars,
        const SizedBox(width: 6),
        Text(
          rating.toStringAsFixed(1),
          style: const TextStyle(fontSize: 12, color: Colors.grey),
        ),
      ],
    );
  }
}
