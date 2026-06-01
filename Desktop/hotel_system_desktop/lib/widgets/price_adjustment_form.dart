import 'package:flutter/material.dart';
import '../models/price_adjustment.dart';
import '../services/price_adjustment_service.dart';

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
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
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
    };
    try {
      if (widget.adjustment == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.adjustment!.id, body);
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
      title: Text(
          widget.adjustment == null ? 'Novo pravilo cijene' : 'Uredi pravilo'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: name,
                decoration: const InputDecoration(labelText: 'Naziv'),
                onChanged: (v) => name = v,
              ),
              TextFormField(
                initialValue: percentageModifier.toString(),
                decoration:
                    const InputDecoration(labelText: 'Modifikator (%)'),
                keyboardType:
                    const TextInputType.numberWithOptions(decimal: true),
                onChanged: (v) => percentageModifier = double.tryParse(v) ?? 0,
              ),
              SwitchListTile(
                title: const Text('Kumulativno'),
                value: isCumulative,
                onChanged: (v) => setState(() => isCumulative = v),
              ),
              if (error != null)
                Text(error!, style: const TextStyle(color: Colors.red)),
            ],
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
