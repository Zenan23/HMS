import 'package:flutter/material.dart';
import '../models/inventory_transaction.dart';
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
  final _staffIdController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _fetchTransactions();
  }

  @override
  void dispose() {
    _itemIdController.dispose();
    _staffIdController.dispose();
    super.dispose();
  }

  Future<void> _fetchTransactions() async {
    setState(() => _isLoading = true);
    try {
      final itemId = int.tryParse(_itemIdController.text);
      final staffId = int.tryParse(_staffIdController.text);
      if (itemId != null) {
        _transactions = await _service.getByInventoryItemId(itemId);
      } else if (staffId != null) {
        _transactions = await _service.getByStaffUserId(staffId);
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
                  width: 140,
                  child: TextField(
                    controller: _staffIdController,
                    decoration:
                        const InputDecoration(labelText: 'ID uposlenika'),
                    keyboardType: TextInputType.number,
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
                            return Card(
                              child: ListTile(
                                title: Text(
                                    'Artikal ${t.inventoryItemId} (${t.quantityChange > 0 ? '+' : ''}${t.quantityChange})'),
                                subtitle: Text(
                                    '${t.reason}\n${t.staffUserName} • ${formatDisplayDate(t.transactionDate)}'),
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
