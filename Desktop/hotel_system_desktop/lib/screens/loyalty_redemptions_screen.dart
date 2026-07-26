import 'package:flutter/material.dart';
import '../models/loyalty_points_redemption.dart';
import '../services/loyalty_points_redemption_service.dart';
import '../utils/date_format_utils.dart';
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
  final int _pageSize = 10;
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
    if (mounted) setState(() => _isLoading = false);
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
      appBar: AppBar(title: const Text('Iskorištenja bodova vjernosti')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        tooltip: 'Novo iskorištenje',
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                SizedBox(
                  width: 140,
                  child: TextField(
                    controller: _userIdController,
                    decoration:
                        const InputDecoration(labelText: 'ID korisnika'),
                    keyboardType: TextInputType.number,
                  ),
                ),
                const SizedBox(width: 8),
                SizedBox(
                  width: 140,
                  child: TextField(
                    controller: _bookingIdController,
                    decoration:
                        const InputDecoration(labelText: 'ID rezervacije'),
                    keyboardType: TextInputType.number,
                  ),
                ),
                const SizedBox(width: 8),
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
                  : _redemptions.isEmpty
                      ? const Center(child: Text('Nema iskorištenja.'))
                      : ListView.builder(
                          itemCount: _redemptions.length,
                          itemBuilder: (context, i) {
                            final r = _redemptions[i];
                            return Card(
                              child: ListTile(
                                title: Text(
                                    '${r.userName} – ${r.pointsUsed} bodova'),
                                subtitle: Text(
                                    'Rezervacija #${r.bookingId} • ${r.equivalentValueAmount.toStringAsFixed(2)} EUR\n'
                                    '${formatDisplayDate(r.redeemedAt)}'),
                                isThreeLine: true,
                                trailing: IconButton(
                                  icon: const Icon(Icons.edit),
                                  tooltip: 'Uredi',
                                  onPressed: () =>
                                      _openForm(redemption: r),
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
