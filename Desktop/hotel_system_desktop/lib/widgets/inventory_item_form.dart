import 'package:flutter/material.dart';
import '../models/inventory_item.dart';
import '../services/inventory_item_service.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';

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

  // Category/Unit su na backendu i dalje slobodan tekst (nema FK tabele —
  // vidi TODO-uskladjenost-db.md), ali ovdje ponudimo postojeće vrijednosti
  // kao prijedloge (combobox) da se izbjegnu duplikati/tipfeleri, umjesto
  // čistog tekstualnog polja.
  List<String> _categorySuggestions = [];
  List<String> _unitSuggestions = [];

  @override
  void initState() {
    super.initState();
    final i = widget.item;
    name = i?.name ?? '';
    unit = i?.unit ?? '';
    category = i?.category ?? '';
    minimumStockLevel = i?.minimumStockLevel ?? 0;
    _fetchSuggestions();
  }

  Future<void> _fetchSuggestions() async {
    try {
      final all = await _service.getAllForDropdown();
      final categories = all
          .map((e) => e.category.trim())
          .where((c) => c.isNotEmpty)
          .toSet()
          .toList()
        ..sort();
      final units = all
          .map((e) => e.unit.trim())
          .where((u) => u.isNotEmpty)
          .toSet()
          .toList()
        ..sort();
      if (mounted) {
        setState(() {
          _categorySuggestions = categories;
          _unitSuggestions = units;
        });
      }
    } catch (_) {
      // ignore — polja i dalje rade kao obično tekstualno polje
    }
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
      setState(() => error = friendlyErrorMessage(e));
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(widget.item == null ? 'Novi artikal' : 'Uredi artikal'),
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
                Autocomplete<String>(
                  initialValue: TextEditingValue(text: unit),
                  optionsBuilder: (v) => v.text.isEmpty
                      ? _unitSuggestions
                      : _unitSuggestions.where((u) =>
                          u.toLowerCase().contains(v.text.toLowerCase())),
                  onSelected: (v) => unit = v,
                  fieldViewBuilder:
                      (context, controller, focusNode, onSubmitted) {
                    return TextFormField(
                      controller: controller,
                      focusNode: focusNode,
                      decoration: const InputDecoration(
                          labelText: 'Jedinica mjere (npr. kom, kg, l)'),
                      onChanged: (v) => unit = v,
                      validator: (v) => (v == null || v.trim().isEmpty)
                          ? 'Jedinica mjere je obavezna.'
                          : null,
                    );
                  },
                ),
                Autocomplete<String>(
                  initialValue: TextEditingValue(text: category),
                  optionsBuilder: (v) => v.text.isEmpty
                      ? _categorySuggestions
                      : _categorySuggestions.where((c) =>
                          c.toLowerCase().contains(v.text.toLowerCase())),
                  onSelected: (v) => category = v,
                  fieldViewBuilder:
                      (context, controller, focusNode, onSubmitted) {
                    return TextFormField(
                      controller: controller,
                      focusNode: focusNode,
                      decoration:
                          const InputDecoration(labelText: 'Kategorija'),
                      onChanged: (v) => category = v,
                    );
                  },
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
