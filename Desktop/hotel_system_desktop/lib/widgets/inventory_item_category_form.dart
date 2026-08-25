import 'package:flutter/material.dart';
import '../models/inventory_item_category.dart';
import '../services/inventory_item_category_service.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';

class InventoryItemCategoryFormDialog extends StatefulWidget {
  final InventoryItemCategory? category;
  const InventoryItemCategoryFormDialog({super.key, this.category});

  @override
  State<InventoryItemCategoryFormDialog> createState() =>
      _InventoryItemCategoryFormDialogState();
}

class _InventoryItemCategoryFormDialogState
    extends State<InventoryItemCategoryFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = InventoryItemCategoryService();
  late String name;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    name = widget.category?.name ?? '';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {'id': widget.category?.id ?? 0, 'name': name};
    try {
      if (widget.category == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.category!.id, body);
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
      title: AppDialogTitle(widget.category == null
          ? 'Nova kategorija artikla'
          : 'Uredi kategoriju artikla'),
      content: Form(
        key: _formKey,
        child: TextFormField(
          initialValue: name,
          autofocus: true,
          decoration: const InputDecoration(labelText: 'Naziv kategorije'),
          onChanged: (v) => name = v,
          validator: (v) =>
              (v == null || v.trim().isEmpty) ? 'Naziv je obavezan.' : null,
        ),
      ),
      actions: [
        if (error != null)
          Padding(
            padding: const EdgeInsets.only(right: 8),
            child: Text(error!, style: const TextStyle(color: Colors.red)),
          ),
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
