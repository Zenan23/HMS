import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/service.dart';
import '../services/api_service.dart';
import '../widgets/service_form.dart';

class ServicesScreen extends StatefulWidget {
  const ServicesScreen({super.key});

  @override
  State<ServicesScreen> createState() => _ServicesScreenState();
}

class _ServicesScreenState extends State<ServicesScreen> {
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  List<Service> _services = [];

  @override
  void initState() {
    super.initState();
    _fetchServices(_page);
  }

  Future<void> _fetchServices(int page) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService()
          .get('/api/Services?pageNumber=$page&pageSize=$_pageSize');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final services = items.map((e) => Service.fromJson(e)).toList();
      setState(() {
        _services = services;
        _page = page;
        int totalCount = data['totalCount'] ?? 0;
        _totalPages = (totalCount / _pageSize).ceil();
      });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Greška: $e')));
      }
    }
    setState(() => _isLoading = false);
  }

  void _openServiceForm({Service? service}) async {
    final result = await showDialog(
      context: context,
      builder: (context) => ServiceFormDialog(service: service),
    );
    if (result == true) {
      _fetchServices(_page);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Servisi')),
      body: Padding(
        padding: const EdgeInsets.all(16.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                ElevatedButton.icon(
                  icon: const Icon(Icons.add),
                  label: const Text('Dodaj servis'),
                  onPressed: _openServiceForm,
                ),
              ],
            ),
            const SizedBox(height: 16),
            Expanded(
              child: SingleChildScrollView(
                scrollDirection: Axis.vertical,
                child: DataTable(
                  columns: const [
                    DataColumn(label: Text('Naziv')),
                    DataColumn(label: Text('Opis')),
                    DataColumn(label: Text('Cijena')),
                    DataColumn(label: Text('Kategorija')),
                    DataColumn(label: Text('Dostupno')),
                    DataColumn(label: Text('Aktivno')),
                    DataColumn(label: Text('Hotel')),
                    DataColumn(label: Text('Kreiran')),
                    DataColumn(label: Text('Ažuriran')),
                    DataColumn(label: Text('Uredi')),
                    DataColumn(label: Text('Obriši')),
                  ],
                  rows: _services
                      .map(
                        (s) => DataRow(
                          cells: [
                            DataCell(Text(s.name)),
                            DataCell(Text(s.description)),
                            DataCell(Text(s.price.toStringAsFixed(2))),
                            DataCell(Text(s.category)),
                            DataCell(Icon(
                                s.isAvailable ? Icons.check : Icons.close,
                                color:
                                    s.isAvailable ? Colors.green : Colors.red)),
                            DataCell(Icon(
                                s.isActive ? Icons.check : Icons.close,
                                color: s.isActive ? Colors.green : Colors.red)),
                            DataCell(Text(s.hotelId.toString())),
                            DataCell(Text(s.createdAt.toString())),
                            DataCell(Text(s.updatedAt.toString())),
                            DataCell(
                              IconButton(
                                icon: const Icon(Icons.edit),
                                tooltip: 'Uredi',
                                onPressed: () => _openServiceForm(service: s),
                              ),
                            ),
                            DataCell(
                              IconButton(
                                icon:
                                    const Icon(Icons.delete, color: Colors.red),
                                tooltip: 'Obriši',
                                onPressed: () async {
                                  final confirm = await showDialog<bool>(
                                    context: context,
                                    builder: (context) => AlertDialog(
                                      title: const Text('Potvrda brisanja'),
                                      content:
                                          Text('Obrisati servis ${s.name}?'),
                                      actions: [
                                        TextButton(
                                            onPressed: () =>
                                                Navigator.pop(context, false),
                                            child: const Text('Ne')),
                                        ElevatedButton(
                                            onPressed: () =>
                                                Navigator.pop(context, true),
                                            child: const Text('Da')),
                                      ],
                                    ),
                                  );
                                  if (confirm == true) {
                                    try {
                                      await ApiService()
                                          .delete('/api/Services/${s.id}');
                                      if (mounted) _fetchServices(_page);
                                    } catch (e) {
                                      if (mounted) {
                                        ScaffoldMessenger.of(context)
                                            .showSnackBar(SnackBar(
                                                content: Text('Greška: $e')));
                                      }
                                    }
                                  }
                                },
                              ),
                            ),
                          ],
                        ),
                      )
                      .toList(),
                ),
              ),
            ),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: _page > 1 && !_isLoading
                      ? () => _fetchServices(_page - 1)
                      : null,
                  child: const Text('Prethodna'),
                ),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0),
                  child: Text('Stranica $_page / $_totalPages'),
                ),
                ElevatedButton(
                  onPressed: _page < _totalPages && !_isLoading
                      ? () => _fetchServices(_page + 1)
                      : null,
                  child: const Text('Sljedeća'),
                ),
              ],
            ),
            if (_isLoading)
              const Padding(
                padding: EdgeInsets.all(16.0),
                child: Center(child: CircularProgressIndicator()),
              ),
          ],
        ),
      ),
    );
  }
}
