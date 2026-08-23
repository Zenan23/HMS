import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/hotel.dart';
import '../models/price_adjustment.dart';
import '../services/api_service.dart';
import '../services/price_adjustment_service.dart';
import '../widgets/date_picker_field.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';

class PriceAdjustmentFormDialog extends StatefulWidget {
  final PriceAdjustment? adjustment;
  const PriceAdjustmentFormDialog({super.key, this.adjustment});

  @override
  State<PriceAdjustmentFormDialog> createState() =>
      _PriceAdjustmentFormDialogState();
}

class _PriceAdjustmentFormDialogState extends State<PriceAdjustmentFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = PriceAdjustmentService();
  late String name;
  late double percentageModifier;
  late DateTime startDate;
  late DateTime endDate;
  late bool isCumulative;
  int? hotelId;
  List<Hotel> _hotels = [];
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final a = widget.adjustment;
    name = a?.name ?? '';
    percentageModifier = a?.percentageModifier ?? 0;
    startDate = a?.startDate ?? DateTime.now();
    endDate = a?.endDate ?? DateTime.now().add(const Duration(days: 30));
    isCumulative = a?.isCumulative ?? false;
    hotelId = a?.hotelId;
    _fetchHotels();
  }

  Future<void> _fetchHotels() async {
    try {
      final response =
          await ApiService().get('/api/Hotels?pageNumber=1&pageSize=100');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      if (mounted) {
        setState(() {
          _hotels = items.map((e) => Hotel.fromJson(e)).toList().cast<Hotel>();
        });
      }
    } catch (_) {
      // ignore, dropdown ostaje samo sa opcijom "Svi hoteli"
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (endDate.isBefore(startDate)) {
      setState(() => error = 'Datum kraja mora biti nakon datuma početka.');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.adjustment?.id ?? 0,
      'name': name,
      'percentageModifier': percentageModifier,
      'startDate': startDate.toIso8601String(),
      'endDate': endDate.toIso8601String(),
      'isCumulative': isCumulative,
      'hotelId': hotelId,
    };
    try {
      if (widget.adjustment == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.adjustment!.id, body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() => error = friendlyErrorMessage(e));
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(
          widget.adjustment == null ? 'Novo pravilo cijene' : 'Uredi pravilo'),
      content: SizedBox(
        width: 420,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextFormField(
                  initialValue: name,
                  decoration: const InputDecoration(labelText: 'Naziv'),
                  validator: (v) =>
                      (v == null || v.trim().isEmpty) ? 'Unesite naziv' : null,
                  onChanged: (v) => name = v,
                ),
                TextFormField(
                  initialValue: percentageModifier.toString(),
                  decoration:
                      const InputDecoration(labelText: 'Modifikator (%)'),
                  keyboardType:
                      const TextInputType.numberWithOptions(decimal: true),
                  onChanged: (v) =>
                      percentageModifier = double.tryParse(v) ?? 0,
                ),
                const SizedBox(height: 8),
                DatePickerField(
                  label: 'Datum početka',
                  value: startDate,
                  onChanged: (d) {
                    if (d != null) setState(() => startDate = d);
                  },
                ),
                const SizedBox(height: 8),
                DatePickerField(
                  label: 'Datum kraja',
                  value: endDate,
                  onChanged: (d) {
                    if (d != null) setState(() => endDate = d);
                  },
                ),
                SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Kumulativno'),
                  value: isCumulative,
                  onChanged: (v) => setState(() => isCumulative = v),
                ),
                DropdownButtonFormField<int?>(
                  value: _hotels.any((h) => h.id == hotelId) ? hotelId : null,
                  decoration: const InputDecoration(
                      labelText: 'Hotel (prazno = važi za sve hotele)'),
                  items: [
                    const DropdownMenuItem<int?>(
                      value: null,
                      child: Text('Svi hoteli (sajt-wide)'),
                    ),
                    ..._hotels.map((h) => DropdownMenuItem<int?>(
                          value: h.id,
                          child: Text(h.name),
                        )),
                  ],
                  onChanged: (v) => setState(() => hotelId = v),
                ),
                if (error != null)
                  Text(error!, style: const TextStyle(color: Colors.red)),
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
