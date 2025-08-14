import 'package:flutter/material.dart';
import '../services/reviews_service.dart';

class RoomDetailScreen extends StatefulWidget {
  final int roomId;
  final String roomName;
  const RoomDetailScreen({Key? key, required this.roomId, required this.roomName}) : super(key: key);

  @override
  State<RoomDetailScreen> createState() => _RoomDetailScreenState();
}

class _RoomDetailScreenState extends State<RoomDetailScreen> {
  final _reviewController = TextEditingController();
  int _rating = 5;
  bool _loading = false;
  String? _error;
  List<dynamic> _reviews = [];

  @override
  void initState() {
    super.initState();
    _fetchReviews();
  }

  Future<void> _fetchReviews() async {
    setState(() { _loading = true; });
    try {
      _reviews = await ReviewsService().fetchReviews(widget.roomId);
    } catch (e) {
      _error = 'Greška pri dohvatu recenzija';
    }
    setState(() { _loading = false; });
  }

  Future<void> _addReview() async {
    setState(() { _loading = true; _error = null; });
    final success = await ReviewsService().addReview(widget.roomId, _reviewController.text, _rating);
    if (success) {
      _reviewController.clear();
      await _fetchReviews();
    } else {
      setState(() { _error = 'Greška pri dodavanju recenzije'; });
    }
    setState(() { _loading = false; });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.roomName)),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Recenzije', style: Theme.of(context).textTheme.headlineLarge),
                  const SizedBox(height: 12),
                  Expanded(
                    child: _reviews.isEmpty
                        ? const Text('Nema recenzija.')
                        : ListView.builder(
                            itemCount: _reviews.length,
                            itemBuilder: (context, i) {
                              final r = _reviews[i];
                              return ListTile(
                                title: Text(r['text'] ?? ''),
                                subtitle: Text('Ocjena: ${r['rating'] ?? ''}'),
                              );
                            },
                          ),
                  ),
                  if (_error != null) ...[
                    Text(_error!, style: const TextStyle(color: Colors.red)),
                    const SizedBox(height: 8),
                  ],
                  TextField(
                    controller: _reviewController,
                    decoration: const InputDecoration(labelText: 'Vaša recenzija'),
                  ),
                  Row(
                    children: [
                      const Text('Ocjena:'),
                      DropdownButton<int>(
                        value: _rating,
                        items: List.generate(5, (i) => i + 1)
                            .map((v) => DropdownMenuItem(value: v, child: Text('$v')))
                            .toList(),
                        onChanged: (v) => setState(() => _rating = v ?? 5),
                      ),
                      const Spacer(),
                      ElevatedButton(
                        onPressed: _loading ? null : _addReview,
                        child: const Text('Dodaj recenziju'),
                      ),
                    ],
                  ),
                ],
              ),
            ),
    );
  }
}