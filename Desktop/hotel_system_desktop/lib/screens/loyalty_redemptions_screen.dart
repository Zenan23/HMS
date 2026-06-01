import 'package:flutter/material.dart';
import '../models/loyalty_points_redemption.dart';
import '../services/loyalty_points_redemption_service.dart';
import '../utils/error_helper.dart';
import '../widgets/loyalty_redemption_form.dart';

class LoyaltyRedemptionsScreen extends StatefulWidget {
  const LoyaltyRedemptionsScreen({super.key});

  @override
  State<LoyaltyRedemptionsScreen> createState() =>
      _LoyaltyRedemptionsScreenState();
}

class _LoyaltyRedemptionsScreenState extends State<LoyaltyRedemptionsScreen> {
  final _service = LoyaltyPointsRedemptionService();
  int _page = 1;
  int _pageSize = 10;
  bool _isLoading = false;
  List<LoyaltyPointsRedemption> _redemptions = [];
  final _userIdController = TextEditingController();
  final _bookingIdController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _fetchRedemptions();
  }

  @override
  void dispose() {
    _userIdController.dispose();
    _bookingIdController.dispose();
    super.dispose();
  }

  Future<void> _fetchRedemptions() async {
    setState(() => _isLoading = true);
    try {
      final userId = int.tryParse(_userIdController.text);
      final bookingId = int.tryParse(_bookingIdController.text);
      if (userId != null) {
        _redemptions = await _service.getByUserId(userId);
      } else if (bookingId != null) {
        _redemptions = await _service.getByBookingId(bookingId);
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        _redemptions = result.items;
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    setState(() => _isLoading = false);
  }

  Future<void> _openForm({LoyaltyPointsRedemption? redemption}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => LoyaltyRedemptionFormDialog(redemption: redemption),
    );
    if (result == true) _fetchRedemptions();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Loyalty iskorištenja')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                SizedBox(
                  width: 120,
                  child: TextField(
                    controller: _userIdController,
                    decoration: const InputDecoration(labelText: 'User ID'),
                    keyboardType: TextInputType.number,
                  ),
                ),
                const SizedBox(width: 8),
                SizedBox(
                  width: 120,
                  child: TextField(
                    controller: _bookingIdController,
                    decoration: const InputDecoration(labelText: 'Booking ID'),
                    keyboardType: TextInputType.number,
                  ),
                ),
                ElevatedButton(
                  onPressed: _fetchRedemptions,
                  child: const Text('Filtriraj'),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : ListView.builder(
                      itemCount: _redemptions.length,
                      itemBuilder: (context, i) {
                        final r = _redemptions[i];
                        return Card(
                          child: ListTile(
                            title: Text('${r.userName} - ${r.pointsUsed} bodova'),
                            subtitle: Text(
                                'Rezervacija #${r.bookingId} • ${r.equivalentValueAmount} EUR\n${r.redeemedAt}'),
                            isThreeLine: true,
                            trailing: IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: () => _openForm(redemption: r),
                            ),
                          ),
                        );
                      },
                    ),
            ),
          ],
        ),
      ),
    );
  }
}
