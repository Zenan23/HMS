import 'package:flutter/material.dart';
import 'dart:convert';
import 'package:provider/provider.dart';
import '../models/hotel.dart';
import '../models/room.dart';
import '../services/hotels_service.dart';
import '../services/auth_service.dart';
import 'room_booking_screen.dart';
import '../services/api_service.dart';

class HotelDetailScreen extends StatefulWidget {
  final Hotel hotel;

  const HotelDetailScreen({Key? key, required this.hotel}) : super(key: key);

  @override
  State<HotelDetailScreen> createState() => _HotelDetailScreenState();
}

class _HotelDetailScreenState extends State<HotelDetailScreen> {
  bool _loadingReviews = false;
  String? _reviewsError;
  List<dynamic> _reviews = [];
  final TextEditingController _reviewController = TextEditingController();
  final TextEditingController _reviewTitleController = TextEditingController();
  int _newRating = 5;
  bool _submittingReview = false;
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _loadHotelRooms();
      _loadHotelReviews();
    });
  }

  @override
  void dispose() {
    _reviewController.dispose();
    _reviewTitleController.dispose();
    super.dispose();
  }

  Future<void> _loadHotelRooms() async {
    final authService = Provider.of<AuthService>(context, listen: false);
    final hotelsService = Provider.of<HotelsService>(context, listen: false);

    final error = await hotelsService.fetchHotelRooms(widget.hotel.id,
        token: authService.token);
    if (error != null && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error)),
      );
    }
  }

  Future<void> _loadHotelReviews() async {
    setState(() {
      _loadingReviews = true;
      _reviewsError = null;
    });
    try {
      final resp = await ApiService.get('/Reviews/hotel/${widget.hotel.id}');
      if (resp.statusCode == 200) {
        final decoded = jsonDecode(resp.body) as Map<String, dynamic>;
        final data = decoded['data'] as List<dynamic>? ?? [];
        setState(() {
          _reviews = data;
        });
      } else {
        setState(() {
          _reviewsError = 'Greška pri dohvatu recenzija';
        });
      }
    } catch (_) {
      setState(() {
        _reviewsError = 'Greška pri dohvatu recenzija';
      });
    } finally {
      if (mounted) {
        setState(() {
          _loadingReviews = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: NestedScrollView(
        headerSliverBuilder: (context, innerBoxIsScrolled) => [
          SliverAppBar(
            pinned: true,
            expandedHeight: 240,
            flexibleSpace: FlexibleSpaceBar(
              title: Text(widget.hotel.name),
              background: widget.hotel.imageUrl.isNotEmpty
                  ? Hero(
                      tag: 'hotel_${widget.hotel.id}',
                      child: LayoutBuilder(
                        builder: (context, constraints) {
                          final dpr = MediaQuery.of(context).devicePixelRatio;
                          final targetWidth = (constraints.maxWidth * dpr).clamp(480, 1600).round();
                          return Image.network(
                            widget.hotel.imageUrl,
                            fit: BoxFit.cover,
                            gaplessPlayback: true,
                            cacheWidth: targetWidth,
                            filterQuality: FilterQuality.low,
                          );
                        },
                      ),
                    )
                  : Container(color: Colors.grey.shade200),
            ),
          ),
        ],
        body: SingleChildScrollView(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 12),
              _buildHotelInfo(),
              _buildRoomsSection(),
              _buildReviewsSection(),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildHotelHeader() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            Theme.of(context).colorScheme.primary.withOpacity(0.1),
            Theme.of(context).colorScheme.secondary.withOpacity(0.1),
          ],
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (widget.hotel.imageUrl.isNotEmpty) ...[
            ClipRRect(
              borderRadius: BorderRadius.circular(12),
              child: Image.network(
                widget.hotel.imageUrl,
                height: 180,
                width: double.infinity,
                fit: BoxFit.cover,
                errorBuilder: (_, __, ___) => Container(
                  height: 180,
                  color: Colors.grey.shade300,
                  child: const Center(child: Icon(Icons.broken_image)),
                ),
              ),
            ),
            const SizedBox(height: 12),
          ],
          Text(
            widget.hotel.name,
            style: const TextStyle(
              fontSize: 28,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              const Icon(Icons.location_on, color: Colors.grey),
              const SizedBox(width: 4),
              Expanded(
                child: Text(
                  '${widget.hotel.address}, ${widget.hotel.city}, ${widget.hotel.country}',
                  style: const TextStyle(fontSize: 16, color: Colors.grey),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          _buildStarRatingRow(widget.hotel.averageRating ?? widget.hotel.starRating.toDouble()),
        ],
      ),
    );
  }

  Widget _buildReviewsSection() {
    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Recenzije',
                style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
              ),
              if (!_loadingReviews)
                IconButton(
                  onPressed: _loadHotelReviews,
                  icon: const Icon(Icons.refresh),
                ),
            ],
          ),
          const SizedBox(height: 12),
          if (_loadingReviews)
            const Center(child: CircularProgressIndicator())
          else if (_reviewsError != null)
            Text(_reviewsError!, style: const TextStyle(color: Colors.red))
          else if (_reviews.isEmpty)
            const Text('Nema recenzija.')
          else
            ListView.builder(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              itemCount: _reviews.length,
              itemBuilder: (context, index) {
                final r = _reviews[index] as Map<String, dynamic>;
                final rating = (r['rating'] as num?)?.toDouble() ?? 0;
                return Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: ListTile(
                    title: Text(r['title'] ?? 'Bez naslova'),
                    subtitle: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        _buildStarRatingRow(rating),
                        const SizedBox(height: 4),
                        Text(r['comment'] ?? ''),
                      ],
                    ),
                  ),
                );
              },
            ),
          const SizedBox(height: 24),
          _buildAddReviewSection(),
        ],
      ),
    );
  }

  Widget _buildAddReviewSection() {
    final user = context.watch<AuthService>().user;
    if (user == null) {
      return const SizedBox.shrink();
    }
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Ostavi recenziju',
          style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
        ),
        const SizedBox(height: 8),
        TextField(
          controller: _reviewTitleController,
          maxLines: 1,
          decoration: const InputDecoration(
            labelText: 'Naslov (opcionalno)',
            border: OutlineInputBorder(),
          ),
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            const Text('Ocjena:'),
            const SizedBox(width: 8),
            DropdownButton<int>(
              value: _newRating,
              items: List.generate(5, (i) => i + 1)
                  .map((v) => DropdownMenuItem(value: v, child: Text('$v')))
                  .toList(),
              onChanged: (v) => setState(() => _newRating = v ?? 5),
            ),
          ],
        ),
        TextField(
          controller: _reviewController,
          maxLines: 3,
          decoration: const InputDecoration(
            labelText: 'Komentar',
            border: OutlineInputBorder(),
          ),
        ),
        const SizedBox(height: 8),
        Align(
          alignment: Alignment.centerRight,
          child: ElevatedButton.icon(
            onPressed: _submittingReview ? null : _submitHotelReview,
            icon: _submittingReview
                ? const SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.send),
            label: const Text('Pošalji'),
          ),
        ),
      ],
    );
  }

  Future<void> _submitHotelReview() async {
    if (_reviewController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Unesite komentar prije slanja.')),
      );
      return;
    }
    final auth = context.read<AuthService>();
    final userId = auth.user?.userId;
    if (userId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Morate biti prijavljeni.')),
      );
      return;
    }
    setState(() => _submittingReview = true);
    try {
      final payload = {
        'rating': _newRating,
        'comment': _reviewController.text.trim(),
        'hotelId': widget.hotel.id,
        'userId': userId,
      };
      final title = _reviewTitleController.text.trim();
      if (title.isNotEmpty) payload['title'] = title;
      final resp = await ApiService.post('/Reviews', payload);
      if (resp.statusCode == 200 || resp.statusCode == 201) {
        _reviewController.clear();
        await _loadHotelReviews();
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Recenzija je dodana.')), 
          );
        }
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Greška pri dodavanju recenzije.')),
        );
      }
    } catch (_) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Greška pri povezivanju sa serverom.')),
      );
    } finally {
      if (mounted) setState(() => _submittingReview = false);
    }
  }

  Widget _buildHotelInfo() {
    return Padding(
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Informacije o hotelu',
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 16),
          if (widget.hotel.description.isNotEmpty) ...[
            _buildInfoRow('Opis', widget.hotel.description),
            const SizedBox(height: 12),
          ],
          _buildInfoRow('Telefon', widget.hotel.phoneNumber),
          const SizedBox(height: 12),
          _buildInfoRow('Email', widget.hotel.email),
          const SizedBox(height: 12),
          _buildInfoRow('Grad', widget.hotel.city),
          const SizedBox(height: 12),
          _buildInfoRow('Zemlja', widget.hotel.country),
        ],
      ),
    );
  }

  Widget _buildRoomsSection() {
    return Consumer<HotelsService>(
      builder: (context, hotelsService, child) {
        return Padding(
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Dostupne sobe',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  if (!hotelsService.isLoading)
                    IconButton(
                      onPressed: _loadHotelRooms,
                      icon: const Icon(Icons.refresh),
                    ),
                ],
              ),
              const SizedBox(height: 16),
              if (hotelsService.isLoading)
                const Center(child: CircularProgressIndicator())
              else if (hotelsService.hotelRooms.isEmpty)
                const Center(
                  child: Padding(
                    padding: EdgeInsets.all(20),
                    child: Text(
                      'Nema dostupnih soba',
                      style: TextStyle(fontSize: 16, color: Colors.grey),
                    ),
                  ),
                )
              else
                ListView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: hotelsService.hotelRooms.length,
                  itemBuilder: (context, index) {
                    final room = hotelsService.hotelRooms[index];
                    return _buildRoomCard(room);
                  },
                ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildRoomCard(Room room) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 2,
      child: InkWell(
        onTap: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => RoomBookingScreen(
                roomId: room.id,
                maxOccupancy: room.maxOccupancy,
              ),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    'Soba ${room.roomNumber}',
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: room.isAvailable ? Colors.green : Colors.red,
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(
                      room.isAvailable ? 'Dostupno' : 'Nedostupno',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 12,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                room.roomTypeString,
                style: const TextStyle(
                  fontSize: 16,
                  color: Colors.grey,
                  fontWeight: FontWeight.w500,
                ),
              ),
              const SizedBox(height: 8),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Row(
                    children: [
                      const Icon(Icons.people, size: 16, color: Colors.grey),
                      const SizedBox(width: 4),
                      Text(
                        'Max ${room.maxOccupancy} osoba',
                        style: const TextStyle(color: Colors.grey),
                      ),
                    ],
                  ),
                  Expanded(
                    child: Text(
                      '${room.pricePerNight.toStringAsFixed(2)} EUR/noć',
                      textAlign: TextAlign.end,
                      overflow: TextOverflow.ellipsis,
                      style: const TextStyle(
                        fontSize: 18,
                        fontWeight: FontWeight.bold,
                        color: Colors.green,
                      ),
                    ),
                  ),
                ],
              ),
              if (room.description.isNotEmpty) ...[
                const SizedBox(height: 8),
                Text(
                  room.description,
                  style: const TextStyle(color: Colors.grey),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildInfoRow(String label, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 80,
          child: Text(
            '$label:',
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              color: Colors.grey,
            ),
          ),
        ),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            value,
            style: const TextStyle(fontSize: 16),
          ),
        ),
      ],
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
      return Icon(icon, color: Colors.amber, size: 24);
    });
    return Row(
      children: [
        ...stars,
        const SizedBox(width: 8),
        Text(
          rating.toStringAsFixed(1),
          style: const TextStyle(color: Colors.grey),
        ),
      ],
    );
  }
}
