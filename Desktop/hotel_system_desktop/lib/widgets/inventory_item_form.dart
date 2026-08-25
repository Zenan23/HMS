import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/inventory_item.dart';
import '../services/api_service.dart';
import '../services/inventory_item_service.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';
import 'inventory_item_category_form.dart';

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
  late int? inventoryItemCategoryId;
  late int minimumStockLevel;
  bool isLoading = false;
  String? error;

  // Unit i dalje nema svoju referentnu tabelu, pa ostaje slobodan tekst sa
  // prijedlozima (combobox) da se izbjegnu duplikati/tipfeleri.
  List<String> _unitSuggestions = [];

  // Kategorija je na backendu FK (InventoryItemCategoryId, obavezan) — isti
  // obrazac kao Service.ServiceCategoryId (vidi widgets/service_form.dart).
  List<Map<String, dynamic>> _categories = [];
  bool _checkingCategories = true;

  @override
  void initState() {
    super.initState();
    final i = widget.item;
    name = i?.name ?? '';
    unit = i?.unit ?? '';
    inventoryItemCategoryId =
        (i != null && i.inventoryItemCategoryId > 0)
            ? i.inventoryItemCategoryId
            : null;
    minimumStockLevel = i?.minimumStockLevel ?? 0;
    _fetchUnitSuggestions();
    _fetchInventoryItemCategories();
  }

  Future<void> _fetchUnitSuggestions() async {
    try {
      final all = await _service.getAllForDropdown();
      final units = all
          .map((e) => e.unit.trim())
          .where((u) => u.isNotEmpty)
          .toSet()
          .toList()
        ..sort();
      if (mounted) {
        setState(() {
          _unitSuggestions = units;
        });
      }
    } catch (_) {
      // ignore — polje i dalje radi kao obično tekstualno polje
    }
  }

  Future<void> _fetchInventoryItemCategories() async {
    try {
      final response = await ApiService().get(
        '/api/InventoryItemCategories?pageNumber=1&pageSize=200',
      );
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final categories = items
          .map((e) => {'id': e['id'], 'name': (e['name'] ?? '').toString()})
          .toList();
      if (mounted) setState(() => _categories = categories);
    } catch (_) {
      // ignore — dropdown ostaje prazan, validator će tražiti odabir
    }
    if (mounted) setState(() => _checkingCategories = false);
  }

  // Inline dodavanje kategorije bez napuštanja forme za artikal (isti obrazac
  // kao Hotel -> Grad / Service -> ServiceCategory).
  Future<void> _addCategoryInline() async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => const InventoryItemCategoryFormDialog(),
    );
    if (result == true) {
      await _fetchInventoryItemCategories();
      if (!mounted) return;
      if (_categories.isNotEmpty) {
        setState(() {
          // Novododana kategorija je obično posljednja u listi po Id-u — odaberi je.
          inventoryItemCategoryId = _categories
              .map((c) => c['id'] as int)
              .reduce((a, b) => a > b ? a : b);
        });
      }
    }
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (inventoryItemCategoryId == null) {
      setState(() => error = 'Odaberite kategoriju');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.item?.id ?? 0,
      'name': name,
      'unit': unit,
      'inventoryItemCategoryId': inventoryItemCategoryId,
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
                _checkingCategories
                    ? const Padding(
                        padding: EdgeInsets.symmetric(vertical: 8),
                        child: LinearProgressIndicator(),
                      )
                    : Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Expanded(
                            child: DropdownButtonFormField<int>(
                              value: _categories.any((c) =>
                                      c['id'] == inventoryItemCategoryId)
                                  ? inventoryItemCategoryId
                                  : null,
                              decoration: const InputDecoration(
                                  labelText: 'Kategorija'),
                              items: _categories
                                  .map(
                                    (c) => DropdownMenuItem<int>(
                                      value: c['id'] as int,
                                      child: Text(c['name'] as String),
                                    ),
                                  )
                                  .toList(),
                              onChanged: (v) => setState(
                                  () => inventoryItemCategoryId = v),
                              validator: (v) =>
                                  v == null ? 'Obavezno' : null,
                            ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.add_circle_outline),
                            tooltip: 'Dodaj novu kategoriju',
                            onPressed: _addCategoryInline,
                          ),
                        ],
                      ),
                if (!_checkingCategories && _categories.isEmpty)
                  const Padding(
                    padding: EdgeInsets.only(top: 4, bottom: 4),
                    child: Text(
                      'Nema nijedne kategorije artikla u bazi kontaktirajte '
                      'administratora da doda barem jednu (InventoryItemCategories).',
                      style: TextStyle(fontSize: 12, color: Colors.orange),
                    ),
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
