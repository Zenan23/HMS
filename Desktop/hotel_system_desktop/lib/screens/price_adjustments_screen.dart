import 'package:flutter/material.dart';
import '../models/price_adjustment.dart';
import '../services/price_adjustment_service.dart';
import '../utils/error_helper.dart';
import '../widgets/price_adjustment_form.dart';

class PriceAdjustmentsScreen extends StatefulWidget {
  const PriceAdjustmentsScreen({super.key});

  @override
  State<PriceAdjustmentsScreen> createState() => _PriceAdjustmentsScreenState();
}

class _PriceAdjustmentsScreenState extends State<PriceAdjustmentsScreen> {
  final _service = PriceAdjustmentService();
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  List<PriceAdjustment> _adjustments = [];
  bool _showActiveOnly = false;

  @override
  void initState() {
    super.initState();
    _fetchAdjustments();
  }

  Future<void> _fetchAdjustments() async {
    setState(() => _isLoading = true);
    try {
      if (_showActiveOnly) {
        final list = await _service.getActive();
        setState(() {
          _adjustments = list;
          _totalPages = 1;
        });
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        setState(() {
          _adjustments = result.items;
          _totalPages = result.totalPages;
        });
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    setState(() => _isLoading = false);
  }

  Future<void> _openForm({PriceAdjustment? adjustment}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => PriceAdjustmentFormDialog(adjustment: adjustment),
    );
    if (result == true) _fetchAdjustments();
  }

  Future<void> _delete(int id) async {
    try {
      await _service.delete(id);
      _fetchAdjustments();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Upravljanje cijenama')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            SwitchListTile(
              title: const Text('Prikaži samo aktivne'),
              value: _showActiveOnly,
              onChanged: (v) {
                setState(() => _showActiveOnly = v);
                _fetchAdjustments();
              },
            ),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : ListView.builder(
                      itemCount: _adjustments.length,
                      itemBuilder: (context, i) {
                        final a = _adjustments[i];
                        return Card(
                          child: ListTile(
                            title: Text(a.name),
                            subtitle: Text(
                                '${a.percentageModifier}% • ${a.startDate.toLocal()} - ${a.endDate.toLocal()}\nKumulativno: ${a.isCumulative ? 'Da' : 'Ne'}'),
                            isThreeLine: true,
                            trailing: Row(
                              mainAxisSize: MainAxisSize.min,
                              children: [
                                IconButton(
                                  icon: const Icon(Icons.edit),
                                  onPressed: () => _openForm(adjustment: a),
                                ),
                                IconButton(
                                  icon: const Icon(Icons.delete),
                                  onPressed: () => _delete(a.id),
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
