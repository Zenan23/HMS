import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/inventory_transaction.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/inventory_transaction_service.dart';
import '../services/pdf_report_service.dart';
import '../utils/date_format_utils.dart';
import '../utils/error_helper.dart';
import '../widgets/inventory_transaction_form.dart';

class InventoryTransactionsScreen extends StatefulWidget {
  const InventoryTransactionsScreen({super.key});

  @override
  State<InventoryTransactionsScreen> createState() =>
      _InventoryTransactionsScreenState();
}

class _InventoryTransactionsScreenState
    extends State<InventoryTransactionsScreen> {
  final _service = InventoryTransactionService();
  int _page = 1;
  final int _pageSize = 10;
  bool _isLoading = false;
  List<InventoryTransaction> _transactions = [];
  final _itemIdController = TextEditingController();
  int? _selectedStaffId;
  List<Employee> _staff = [];

  @override
  void initState() {
    super.initState();
    _fetchStaff();
    _fetchTransactions();
  }

  @override
  void dispose() {
    _itemIdController.dispose();
    super.dispose();
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

  Future<void> _fetchTransactions() async {
    setState(() => _isLoading = true);
    try {
      final itemId = int.tryParse(_itemIdController.text);
      if (itemId != null) {
        _transactions = await _service.getByInventoryItemId(itemId);
      } else if (_selectedStaffId != null) {
        _transactions = await _service.getByStaffUserId(_selectedStaffId!);
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        _transactions = result.items;
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _openForm({InventoryTransaction? transaction}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => InventoryTransactionFormDialog(transaction: transaction),
    );
    if (result == true) _fetchTransactions();
  }

  String _staffLabel(Employee u) {
    final name = u.fullName.isNotEmpty ? u.fullName : u.username;
    return name;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Skladišne transakcije'),
        actions: [
          IconButton(
            icon: const Icon(Icons.picture_as_pdf),
            tooltip: 'PDF izvještaj',
            onPressed: () => PdfReportService.exportInventoryTransactions(
                context, _transactions),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        tooltip: 'Nova transakcija',
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                SizedBox(
                  width: 140,
                  child: TextField(
                    controller: _itemIdController,
                    decoration:
                        const InputDecoration(labelText: 'ID artikla'),
                    keyboardType: TextInputType.number,
                  ),
                ),
                const SizedBox(width: 8),
                SizedBox(
                  width: 220,
                  child: DropdownButtonFormField<int?>(
                    value: _selectedStaffId,
                    decoration:
                        const InputDecoration(labelText: 'Uposlenik'),
                    items: [
                      const DropdownMenuItem<int?>(
                        value: null,
                        child: Text('Svi uposlenici'),
                      ),
                      ..._staff.map((u) => DropdownMenuItem<int?>(
                            value: u.id,
                            child: Text(_staffLabel(u)),
                          )),
                    ],
                    onChanged: (v) => setState(() => _selectedStaffId = v),
                  ),
                ),
                const SizedBox(width: 8),
                ElevatedButton(
                  onPressed: _fetchTransactions,
                  child: const Text('Filtriraj'),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _transactions.isEmpty
                      ? const Center(child: Text('Nema transakcija.'))
                      : ListView.builder(
                          itemCount: _transactions.length,
                          itemBuilder: (context, i) {
                            final t = _transactions[i];
                            final staff = t.staffUserName.isNotEmpty
                                ? t.staffUserName
                                : 'Uposlenik #${t.staffUserId}';
                            return Card(
                              child: ListTile(
                                title: Text(
                                    'Artikal #${t.inventoryItemId} (${t.quantityChange > 0 ? '+' : ''}${t.quantityChange})'),
                                subtitle: Text(
                                    '${t.reason}\n$staff • ${formatDisplayDate(t.transactionDate)}'),
                                isThreeLine: true,
                                trailing: IconButton(
                                  icon: const Icon(Icons.edit),
                                  tooltip: 'Uredi',
                                  onPressed: () =>
                                      _openForm(transaction: t),
                                ),
                              ),
                            );
                          },
                        ),
            ),
          ],
        ),
      ),
    );
  }
}
