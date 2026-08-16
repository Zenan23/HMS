import 'package:flutter/material.dart';
import '../models/inventory_item.dart';
import '../services/inventory_item_service.dart';

class InventoryItemFormDialog extends StatefulWidget {
  final InventoryItem? item;
  const InventoryItemFormDialog({super.key, this.item});

  @override
  State<InventoryItemFormDialog> createState() =>
      _InventoryItemFormDialogState();
}

class _InventoryItemFormDialogState extends State<InventoryItemFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = InventoryItemService();
  late String name;
  late String unit;
  late String category;
  late int minimumStockLevel;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final i = widget.item;
    name = i?.name ?? '';
    unit = i?.unit ?? '';
    category = i?.category ?? '';
    minimumStockLevel = i?.minimumStockLevel ?? 0;
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.item?.id ?? 0,
      'name': name,
      'unit': unit,
      'category': category,
      'minimumStockLevel': minimumStockLevel,
    };
    try {
      if (widget.item == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.item!.id, body);
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
      title: Text(widget.item == null ? 'Novi artikal' : 'Uredi artikal'),
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
                  decoration: const InputDecoration(labelText: 'Naziv artikla'),
                  onChanged: (v) => name = v,
                  validator: (v) => (v == null || v.trim().isEmpty)
                      ? 'Naziv je obavezan.'
                      : null,
                ),
                TextFormField(
                  initialValue: unit,
                  decoration: const InputDecoration(
                      labelText: 'Jedinica mjere (npr. kom, kg, l)'),
                  onChanged: (v) => unit = v,
                  validator: (v) => (v == null || v.trim().isEmpty)
                      ? 'Jedinica mjere je obavezna.'
                      : null,
                ),
                TextFormField(
                  initialValue: category,
                  decoration: const InputDecoration(labelText: 'Kategorija'),
                  onChanged: (v) => category = v,
                ),
                TextFormField(
                  initialValue: minimumStockLevel.toString(),
                  decoration:
                      const InputDecoration(labelText: 'Minimalna zaliha'),
                  keyboardType: TextInputType.number,
                  onChanged: (v) =>
                      minimumStockLevel = int.tryParse(v) ?? 0,
                  validator: (v) {
                    final n = int.tryParse(v ?? '');
                    if (n == null || n < 0) {
                      return 'Unesite validan broj (0 ili veći).';
                    }
                    return null;
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
