import 'package:flutter/material.dart';
import '../models/loyalty_points_redemption.dart';
import '../services/loyalty_points_redemption_service.dart';
import 'date_picker_field.dart';

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

  @override
  void initState() {
    super.initState();
    final r = widget.redemption;
    userId = r?.userId ?? 1;
    bookingId = r?.bookingId ?? 1;
    pointsUsed = r?.pointsUsed ?? 100;
    redeemedAt = r?.redeemedAt ?? DateTime.now();
    equivalentValueAmount = r?.equivalentValueAmount ?? 0;
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
      title: Text(widget.redemption == null
          ? 'Novo iskorištenje bodova'
          : 'Uredi iskorištenje'),
      content: SizedBox(
        width: 420,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextFormField(
                  initialValue: userId.toString(),
                  decoration: const InputDecoration(labelText: 'ID korisnika'),
                  keyboardType: TextInputType.number,
                  onChanged: (v) => userId = int.tryParse(v) ?? userId,
                ),
                TextFormField(
                  initialValue: bookingId.toString(),
                  decoration:
                      const InputDecoration(labelText: 'ID rezervacije'),
                  keyboardType: TextInputType.number,
                  onChanged: (v) => bookingId = int.tryParse(v) ?? bookingId,
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
