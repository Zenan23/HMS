import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/inventory_item.dart';
import '../models/inventory_transaction.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/inventory_item_service.dart';
import '../services/inventory_transaction_service.dart';
import 'date_picker_field.dart';

class InventoryTransactionFormDialog extends StatefulWidget {
  final InventoryTransaction? transaction;
  const InventoryTransactionFormDialog({super.key, this.transaction});

  @override
  State<InventoryTransactionFormDialog> createState() =>
      _InventoryTransactionFormDialogState();
}

class _InventoryTransactionFormDialogState
    extends State<InventoryTransactionFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = InventoryTransactionService();
  final _itemService = InventoryItemService();
  int? inventoryItemId;
  late int quantityChange;
  late DateTime transactionDate;
  late int staffUserId;
  late String reason;
  bool isLoading = false;
  bool _isLoadingItems = true;
  String? error;
  List<Employee> _staff = [];
  List<InventoryItem> _items = [];

  @override
  void initState() {
    super.initState();
    final t = widget.transaction;
    inventoryItemId = t?.inventoryItemId;
    quantityChange = t?.quantityChange ?? -1;
    transactionDate = t?.transactionDate ?? DateTime.now();
    staffUserId = t?.staffUserId ?? 0;
    reason = t?.reason ?? '';
    _fetchStaff();
    _fetchItems();
  }

  Future<void> _fetchStaff() async {
    try {
      final resp = await ApiService().get('/api/Users/role/1');
      final decoded = jsonDecode(resp.body);
      final List items = (decoded['data'] ?? []) as List;
      _staff = items.map((e) => Employee.fromJson(e)).toList().cast<Employee>();
    } catch (_) {}
    if (mounted) setState(() {});
  }

  Future<void> _fetchItems() async {
    try {
      _items = await _itemService.getAllForDropdown();
    } catch (_) {}
    if (mounted) setState(() => _isLoadingItems = false);
  }

  String _staffLabel(Employee u) {
    final name = u.fullName.isNotEmpty ? u.fullName : u.username;
    return '$name (${u.email})';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (inventoryItemId == null) {
      setState(() => error = 'Odaberite artikal skladišta.');
      return;
    }
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.transaction?.id ?? 0,
      'inventoryItemId': inventoryItemId,
      'quantityChange': quantityChange,
      'transactionDate': transactionDate.toIso8601String(),
      'staffUserId': staffUserId,
      'reason': reason,
    };
    try {
      if (widget.transaction == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.transaction!.id, body);
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
      title: Text(widget.transaction == null
          ? 'Nova transakcija'
          : 'Uredi transakciju'),
      content: SizedBox(
        width: 420,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                _isLoadingItems
                    ? const Padding(
                        padding: EdgeInsets.symmetric(vertical: 8),
                        child: LinearProgressIndicator(),
                      )
                    : DropdownButtonFormField<int>(
                        value: _items.any((i) => i.id == inventoryItemId)
                            ? inventoryItemId
                            : null,
                        decoration:
                            const InputDecoration(labelText: 'Artikal skladišta'),
                        items: _items
                            .map((i) => DropdownMenuItem<int>(
                                  value: i.id,
                                  child: Text('${i.name} (${i.unit})'),
                                ))
                            .toList(),
                        onChanged: (v) => setState(() => inventoryItemId = v),
                        validator: (v) =>
                            v == null ? 'Odaberite artikal.' : null,
                      ),
                TextFormField(
                  initialValue: quantityChange.toString(),
                  decoration:
                      const InputDecoration(labelText: 'Promjena količine'),
                  keyboardType: TextInputType.number,
                  onChanged: (v) =>
                      quantityChange = int.tryParse(v) ?? quantityChange,
                ),
                DropdownButtonFormField<int>(
                  value: _staff.any((u) => u.id == staffUserId) ? staffUserId : null,
                  decoration: const InputDecoration(labelText: 'Uposlenik'),
                  items: _staff
                      .map((u) => DropdownMenuItem<int>(
                            value: u.id,
                            child: Text(_staffLabel(u)),
                          ))
                      .toList(),
                  onChanged: (v) => setState(() => staffUserId = v ?? 0),
                  validator: (v) =>
                      (v == null || v == 0) ? 'Odaberite uposlenika' : null,
                ),
                TextFormField(
                  initialValue: reason,
                  decoration: const InputDecoration(labelText: 'Razlog'),
                  onChanged: (v) => reason = v,
                ),
                const SizedBox(height: 8),
                DatePickerField(
                  label: 'Datum transakcije',
                  value: transactionDate,
                  onChanged: (d) {
                    if (d != null) setState(() => transactionDate = d);
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
