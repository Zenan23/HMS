import 'package:flutter/material.dart';
import '../models/service_category.dart';
import '../models/inventory_item_category.dart';
import '../services/service_category_service.dart';
import '../services/inventory_item_category_service.dart';
import '../utils/error_helper.dart';
import '../widgets/service_category_form.dart';
import '../widgets/inventory_item_category_form.dart';
import '../widgets/app_dialog_title.dart';

/// Upravljanje referentnim/šifarnik tabelama ServiceCategory/InventoryItemCategory
/// — dropdown izvori za Service.ServiceCategoryId i InventoryItem.InventoryItemCategoryId
/// (zamjena za slobodan tekstualni unos kategorije). Isti obrazac kao CitiesScreen
/// (Grad/Država).
class CategoriesScreen extends StatefulWidget {
  const CategoriesScreen({super.key});

  @override
  State<CategoriesScreen> createState() => _CategoriesScreenState();
}

class _CategoriesScreenState extends State<CategoriesScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final _serviceCategoryService = ServiceCategoryService();
  final _inventoryItemCategoryService = InventoryItemCategoryService();
  bool _isLoadingServiceCategories = false;
  bool _isLoadingInventoryCategories = false;
  List<ServiceCategory> _serviceCategories = [];
  List<InventoryItemCategory> _inventoryCategories = [];
  final _searchController = TextEditingController();
  String _searchTerm = '';

  List<ServiceCategory> get _filteredServiceCategories {
    final q = _searchTerm.trim().toLowerCase();
    if (q.isEmpty) return _serviceCategories;
    return _serviceCategories
        .where((c) => c.name.toLowerCase().contains(q))
        .toList();
  }

  List<InventoryItemCategory> get _filteredInventoryCategories {
    final q = _searchTerm.trim().toLowerCase();
    if (q.isEmpty) return _inventoryCategories;
    return _inventoryCategories
        .where((c) => c.name.toLowerCase().contains(q))
        .toList();
  }

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
    _tabController.addListener(() => setState(() {}));
    _fetchServiceCategories();
    _fetchInventoryCategories();
  }

  @override
  void dispose() {
    _tabController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _fetchServiceCategories() async {
    setState(() => _isLoadingServiceCategories = true);
    try {
      final categories = await _serviceCategoryService.getAllForDropdown();
      if (mounted) setState(() => _serviceCategories = categories);
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoadingServiceCategories = false);
  }

  Future<void> _fetchInventoryCategories() async {
    setState(() => _isLoadingInventoryCategories = true);
    try {
      final categories =
          await _inventoryItemCategoryService.getAllForDropdown();
      if (mounted) setState(() => _inventoryCategories = categories);
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoadingInventoryCategories = false);
  }

  Future<void> _openServiceCategoryForm({ServiceCategory? category}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => ServiceCategoryFormDialog(category: category),
    );
    if (result == true) _fetchServiceCategories();
  }

  Future<void> _openInventoryCategoryForm(
      {InventoryItemCategory? category}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => InventoryItemCategoryFormDialog(category: category),
    );
    if (result == true) _fetchInventoryCategories();
  }

  Future<void> _deleteServiceCategory(ServiceCategory category) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const AppDialogTitle('Potvrda brisanja'),
        content: Text(
            'Obrisati kategoriju usluge „${category.name}”? Nije moguće ako postoje usluge vezane za nju.'),
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
      await _serviceCategoryService.delete(category.id);
      _fetchServiceCategories();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  Future<void> _deleteInventoryCategory(InventoryItemCategory category) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const AppDialogTitle('Potvrda brisanja'),
        content: Text(
            'Obrisati kategoriju artikla „${category.name}”? Nije moguće ako postoje artikli vezani za nju.'),
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
      await _inventoryItemCategoryService.delete(category.id);
      _fetchInventoryCategories();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Kategorije'),
        bottom: TabBar(
          controller: _tabController,
          tabs: const [
            Tab(text: 'Kategorije usluga'),
            Tab(text: 'Kategorije artikala'),
          ],
        ),
      ),
      floatingActionButton: AnimatedBuilder(
        animation: _tabController,
        builder: (context, _) {
          return FloatingActionButton(
            onPressed: () => _tabController.index == 0
                ? _openServiceCategoryForm()
                : _openInventoryCategoryForm(),
            tooltip: _tabController.index == 0
                ? 'Nova kategorija usluge'
                : 'Nova kategorija artikla',
            child: const Icon(Icons.add),
          );
        },
      ),
      body: Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(12, 12, 12, 0),
            child: TextField(
              controller: _searchController,
              decoration: InputDecoration(
                labelText: 'Pretraga kategorija',
                prefixIcon: const Icon(Icons.search),
                border: const OutlineInputBorder(),
                suffixIcon: _searchTerm.isNotEmpty
                    ? IconButton(
                        icon: const Icon(Icons.clear),
                        tooltip: 'Očisti pretragu',
                        onPressed: () {
                          _searchController.clear();
                          setState(() => _searchTerm = '');
                        },
                      )
                    : null,
              ),
              onChanged: (v) => setState(() => _searchTerm = v),
            ),
          ),
          Expanded(
            child: TabBarView(
              controller: _tabController,
              children: [
                _isLoadingServiceCategories
                    ? const Center(child: CircularProgressIndicator())
                    : _filteredServiceCategories.isEmpty
                        ? Center(
                            child: Text(_searchTerm.isNotEmpty
                                ? 'Nema rezultata za pretragu.'
                                : 'Nema kategorija usluga.'))
                        : ListView.builder(
                            itemCount: _filteredServiceCategories.length,
                            itemBuilder: (context, i) {
                              final category = _filteredServiceCategories[i];
                              return Card(
                                margin: const EdgeInsets.symmetric(
                                    horizontal: 12, vertical: 4),
                                child: ListTile(
                                  title: Text(category.name),
                                  trailing: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      IconButton(
                                        icon: const Icon(Icons.edit),
                                        tooltip: 'Uredi',
                                        onPressed: () => _openServiceCategoryForm(
                                            category: category),
                                      ),
                                      IconButton(
                                        icon: const Icon(Icons.delete),
                                        tooltip: 'Obriši',
                                        onPressed: () =>
                                            _deleteServiceCategory(category),
                                      ),
                                    ],
                                  ),
                                ),
                              );
                            },
                          ),
                _isLoadingInventoryCategories
                    ? const Center(child: CircularProgressIndicator())
                    : _filteredInventoryCategories.isEmpty
                        ? Center(
                            child: Text(_searchTerm.isNotEmpty
                                ? 'Nema rezultata za pretragu.'
                                : 'Nema kategorija artikala.'))
                        : ListView.builder(
                            itemCount: _filteredInventoryCategories.length,
                            itemBuilder: (context, i) {
                              final category =
                                  _filteredInventoryCategories[i];
                              return Card(
                                margin: const EdgeInsets.symmetric(
                                    horizontal: 12, vertical: 4),
                                child: ListTile(
                                  title: Text(category.name),
                                  trailing: Row(
                                    mainAxisSize: MainAxisSize.min,
                                    children: [
                                      IconButton(
                                        icon: const Icon(Icons.edit),
                                        tooltip: 'Uredi',
                                        onPressed: () =>
                                            _openInventoryCategoryForm(
                                                category: category),
                                      ),
                                      IconButton(
                                        icon: const Icon(Icons.delete),
                                        tooltip: 'Obriši',
                                        onPressed: () =>
                                            _deleteInventoryCategory(category),
                                      ),
                                    ],
                                  ),
                                ),
                              );
                            },
                          ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
