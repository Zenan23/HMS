import 'package:flutter/material.dart';
import '../models/price_adjustment.dart';
import '../services/pdf_report_service.dart';
import '../services/price_adjustment_service.dart';
import '../utils/date_format_utils.dart';
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
  final int _pageSize = 10;
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
          _page = 1;
          _totalPages = 1;
        });
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        setState(() {
          _adjustments = result.items;
          _totalPages = result.totalPages < 1 ? 1 : result.totalPages;
        });
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _openForm({PriceAdjustment? adjustment}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => PriceAdjustmentFormDialog(adjustment: adjustment),
    );
    if (result == true) _fetchAdjustments();
  }

  Future<void> _delete(PriceAdjustment a) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Potvrda brisanja'),
        content: Text('Obrisati pravilo „${a.name}”?'),
        actions: [
          TextButton(
              onPressed: () => Navigator.pop(context, false),
              child: const Text('Ne')),
          ElevatedButton(
              onPressed: () => Navigator.pop(context, true),
              child: const Text('Da')),
        ],
      ),
    );
    if (confirm != true) return;
    try {
      await _service.delete(a.id);
      _fetchAdjustments();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Upravljanje cijenama'),
        actions: [
          IconButton(
            icon: const Icon(Icons.picture_as_pdf),
            tooltip: 'PDF izvještaj',
            onPressed: () => PdfReportService.exportPriceAdjustments(
                context, _adjustments),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        tooltip: 'Novo pravilo',
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                Expanded(
                  child: SwitchListTile(
                    contentPadding: EdgeInsets.zero,
                    title: const Text('Prikaži samo aktivne'),
                    value: _showActiveOnly,
                    onChanged: (v) {
                      setState(() {
                        _showActiveOnly = v;
                        _page = 1;
                      });
                      _fetchAdjustments();
                    },
                  ),
                ),
                if (!_showActiveOnly)
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.chevron_left),
                        onPressed: _page > 1 && !_isLoading
                            ? () {
                                setState(() => _page--);
                                _fetchAdjustments();
                              }
                            : null,
                      ),
                      Text('Strana $_page / $_totalPages'),
                      IconButton(
                        icon: const Icon(Icons.chevron_right),
                        onPressed: _page < _totalPages && !_isLoading
                            ? () {
                                setState(() => _page++);
                                _fetchAdjustments();
                              }
                            : null,
                      ),
                    ],
                  ),
              ],
            ),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _adjustments.isEmpty
                      ? const Center(child: Text('Nema pravila cijene.'))
                      : ListView.builder(
                          itemCount: _adjustments.length,
                          itemBuilder: (context, i) {
                            final a = _adjustments[i];
                            final scope = a.hotelId != null && a.hotelName.isNotEmpty
                                ? a.hotelName
                                : 'Svi hoteli';
                            return Card(
                              child: ListTile(
                                title: Text(a.name),
                                subtitle: Text(
                                  '${a.percentageModifier}% • '
                                  '${formatDisplayDate(a.startDate)} – ${formatDisplayDate(a.endDate)}\n'
                                  'Kumulativno: ${a.isCumulative ? 'Da' : 'Ne'} • $scope',
                                ),
                                isThreeLine: true,
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.edit),
                                      tooltip: 'Uredi',
                                      onPressed: () =>
                                          _openForm(adjustment: a),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.delete),
                                      tooltip: 'Obriši',
                                      onPressed: () => _delete(a),
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
