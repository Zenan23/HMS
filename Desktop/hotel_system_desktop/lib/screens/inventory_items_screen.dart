import 'package:flutter/material.dart';
import '../models/inventory_item.dart';
import '../services/inventory_item_service.dart';
import '../utils/error_helper.dart';
import '../widgets/inventory_item_form.dart';

class InventoryItemsScreen extends StatefulWidget {
  const InventoryItemsScreen({super.key});

  @override
  State<InventoryItemsScreen> createState() => _InventoryItemsScreenState();
}

class _InventoryItemsScreenState extends State<InventoryItemsScreen> {
  final _service = InventoryItemService();
  int _page = 1;
  final int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  List<InventoryItem> _items = [];

  @override
  void initState() {
    super.initState();
    _fetchItems();
  }

  Future<void> _fetchItems() async {
    setState(() => _isLoading = true);
    try {
      final result =
          await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
      setState(() {
        _items = result.items;
        _totalPages = result.totalPages < 1 ? 1 : result.totalPages;
      });
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _openForm({InventoryItem? item}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => InventoryItemFormDialog(item: item),
    );
    if (result == true) _fetchItems();
  }

  Future<void> _delete(InventoryItem item) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Potvrda brisanja'),
        content: Text(
            'Obrisati artikal „${item.name}”? Ova akcija se ne može poništiti.'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Ne')),
          ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Da, obriši')),
        ],
      ),
    );
    if (confirm != true) return;
    try {
      await _service.delete(item.id);
      _fetchItems();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Artikli skladišta')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        tooltip: 'Novi artikal',
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                IconButton(
                  icon: const Icon(Icons.chevron_left),
                  onPressed: _page > 1 && !_isLoading
                      ? () {
                          setState(() => _page--);
                          _fetchItems();
                        }
                      : null,
                ),
                Text('Strana $_page / $_totalPages'),
                IconButton(
                  icon: const Icon(Icons.chevron_right),
                  onPressed: _page < _totalPages && !_isLoading
                      ? () {
                          setState(() => _page++);
                          _fetchItems();
                        }
                      : null,
                ),
              ],
            ),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _items.isEmpty
                      ? const Center(child: Text('Nema artikala skladišta.'))
                      : ListView.builder(
                          itemCount: _items.length,
                          itemBuilder: (context, i) {
                            final item = _items[i];
                            return Card(
                              child: ListTile(
                                title: Text(item.name),
                                subtitle: Text(
                                  '${item.category.isNotEmpty ? '${item.category} • ' : ''}'
                                  'Jedinica: ${item.unit} • Min. zaliha: ${item.minimumStockLevel}',
                                ),
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.edit),
                                      tooltip: 'Uredi',
                                      onPressed: () => _openForm(item: item),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.delete),
                                      tooltip: 'Obriši',
                                      onPressed: () => _delete(item),
                                    ),
                                  ],
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
