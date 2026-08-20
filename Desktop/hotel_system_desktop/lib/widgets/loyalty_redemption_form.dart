import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/booking.dart';
import '../models/loyalty_points_redemption.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/loyalty_points_redemption_service.dart';
import '../utils/date_format_utils.dart';
import 'date_picker_field.dart';
import 'app_dialog_title.dart';

class LoyaltyRedemptionFormDialog extends StatefulWidget {
  final LoyaltyPointsRedemption? redemption;
  const LoyaltyRedemptionFormDialog({super.key, this.redemption});

  @override
  State<LoyaltyRedemptionFormDialog> createState() =>
      _LoyaltyRedemptionFormDialogState();
}

class _LoyaltyRedemptionFormDialogState
    extends State<LoyaltyRedemptionFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = LoyaltyPointsRedemptionService();
  late int userId;
  late int bookingId;
  late int pointsUsed;
  late DateTime redeemedAt;
  late double equivalentValueAmount;
  bool isLoading = false;
  String? error;
  List<Employee> _users = [];
  List<Booking> _bookings = [];

  @override
  void initState() {
    super.initState();
    final r = widget.redemption;
    userId = r?.userId ?? 0;
    bookingId = r?.bookingId ?? 0;
    pointsUsed = r?.pointsUsed ?? 100;
    redeemedAt = r?.redeemedAt ?? DateTime.now();
    equivalentValueAmount = r?.equivalentValueAmount ?? 0;
    _fetchLookups();
  }

  Future<void> _fetchLookups() async {
    try {
      final guestsResp =
          await ApiService().get('/api/Users/role/0');
      final guestsDecoded = jsonDecode(guestsResp.body);
      final List guestItems = (guestsDecoded['data'] ?? []) as List;
      _users = guestItems.map((e) => Employee.fromJson(e)).toList().cast<Employee>();
    } catch (_) {}
    try {
      final bookingsResp =
          await ApiService().get('/api/Bookings?pageNumber=1&pageSize=100');
      final bookingsDecoded = jsonDecode(bookingsResp.body) as Map<String, dynamic>;
      final items = (bookingsDecoded['data']?['items'] as List?) ?? [];
      _bookings = items.map((e) => Booking.fromJson(e)).toList().cast<Booking>();
    } catch (_) {}
    if (mounted) setState(() {});
  }

  String _userLabel(Employee u) {
    final name = u.fullName.isNotEmpty ? u.fullName : u.username;
    return '$name (${u.email})';
  }

  String _bookingLabel(Booking b) {
    if (b.roomNumber.isNotEmpty || b.userName.isNotEmpty) {
      final room = b.roomNumber.isNotEmpty ? b.roomNumber : '#${b.roomId}';
      final user = b.userName.isNotEmpty ? b.userName : 'Korisnik #${b.userId}';
      return 'BK-${b.id.toString().padLeft(6, '0')} · $room · $user';
    }
    return 'BK-${b.id.toString().padLeft(6, '0')} (${formatDisplayDate(b.checkInDate)})';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.redemption?.id ?? 0,
      'userId': userId,
      'bookingId': bookingId,
      'pointsUsed': pointsUsed,
      'redeemedAt': redeemedAt.toIso8601String(),
      'equivalentValueAmount': equivalentValueAmount,
    };
    try {
      if (widget.redemption == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.redemption!.id, body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() => error = e.toString());
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(widget.redemption == null
          ? 'Novo iskorištenje bodova'
          : 'Uredi iskorištenje'),
      content: SizedBox(
        width: 480,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                DropdownButtonFormField<int>(
                  value: _users.any((u) => u.id == userId) ? userId : null,
                  decoration: const InputDecoration(labelText: 'Korisnik'),
                  items: _users
                      .map((u) => DropdownMenuItem<int>(
                            value: u.id,
                            child: Text(_userLabel(u)),
                          ))
                      .toList(),
                  onChanged: (v) => setState(() => userId = v ?? 0),
                  validator: (v) =>
                      (v == null || v == 0) ? 'Odaberite korisnika' : null,
                ),
                DropdownButtonFormField<int>(
                  value: _bookings.any((b) => b.id == bookingId) ? bookingId : null,
                  decoration: const InputDecoration(labelText: 'Rezervacija'),
                  items: _bookings
                      .map((b) => DropdownMenuItem<int>(
                            value: b.id,
                            child: Text(_bookingLabel(b)),
                          ))
                      .toList(),
                  onChanged: (v) => setState(() => bookingId = v ?? 0),
                  validator: (v) =>
                      (v == null || v == 0) ? 'Odaberite rezervaciju' : null,
                ),
                TextFormField(
                  initialValue: pointsUsed.toString(),
                  decoration: const InputDecoration(labelText: 'Bodova'),
                  keyboardType: TextInputType.number,
                  onChanged: (v) => pointsUsed = int.tryParse(v) ?? pointsUsed,
                ),
                TextFormField(
                  initialValue: equivalentValueAmount.toString(),
                  decoration: const InputDecoration(
                      labelText: 'Ekvivalentna vrijednost (EUR)'),
                  keyboardType:
                      const TextInputType.numberWithOptions(decimal: true),
                  onChanged: (v) =>
                      equivalentValueAmount = double.tryParse(v) ?? 0,
                ),
                const SizedBox(height: 8),
                DatePickerField(
                  label: 'Datum iskorištenja',
                  value: redeemedAt,
                  onChanged: (d) {
                    if (d != null) setState(() => redeemedAt = d);
                  },
                ),
                if (error != null)
                  Padding(
                    padding: const EdgeInsets.only(top: 8),
                    child: Text(error!,
                        style: const TextStyle(color: Colors.red)),
                  ),
              ],
            ),
          ),
        ),
      ),
      actions: [
        TextButton(
            onPressed: () => Navigator.pop(context),
            child: const Text('Otkaži')),
        ElevatedButton(
          onPressed: isLoading ? null : _submit,
          child: isLoading
              ? const SizedBox(
                  width: 20, height: 20, child: CircularProgressIndicator())
              : const Text('Spasi'),
        ),
      ],
    );
  }
}
