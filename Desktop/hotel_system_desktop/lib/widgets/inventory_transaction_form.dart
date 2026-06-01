import 'package:flutter/material.dart';
import '../models/inventory_transaction.dart';
import '../services/inventory_transaction_service.dart';

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
  late int inventoryItemId;
  late int quantityChange;
  late DateTime transactionDate;
  late int staffUserId;
  late String reason;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final t = widget.transaction;
    inventoryItemId = t?.inventoryItemId ?? 1;
    quantityChange = t?.quantityChange ?? -1;
    transactionDate = t?.transactionDate ?? DateTime.now();
    staffUserId = t?.staffUserId ?? 1;
    reason = t?.reason ?? '';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
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
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: inventoryItemId.toString(),
                decoration: const InputDecoration(labelText: 'Inventory Item ID'),
                keyboardType: TextInputType.number,
                onChanged: (v) =>
                    inventoryItemId = int.tryParse(v) ?? inventoryItemId,
              ),
              TextFormField(
                initialValue: quantityChange.toString(),
                decoration: const InputDecoration(labelText: 'Promjena količine'),
                keyboardType: TextInputType.number,
                onChanged: (v) =>
                    quantityChange = int.tryParse(v) ?? quantityChange,
              ),
              TextFormField(
                initialValue: staffUserId.toString(),
                decoration: const InputDecoration(labelText: 'Staff User ID'),
                keyboardType: TextInputType.number,
                onChanged: (v) => staffUserId = int.tryParse(v) ?? staffUserId,
              ),
              TextFormField(
                initialValue: reason,
                decoration: const InputDecoration(labelText: 'Razlog'),
                onChanged: (v) => reason = v,
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
