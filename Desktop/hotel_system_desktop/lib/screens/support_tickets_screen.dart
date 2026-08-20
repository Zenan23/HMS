import 'package:flutter/material.dart';
import '../models/support_ticket.dart';
import '../services/pdf_report_service.dart';
import '../services/support_ticket_service.dart';
import '../utils/error_helper.dart';
import '../widgets/support_ticket_form.dart';

class SupportTicketsScreen extends StatefulWidget {
  const SupportTicketsScreen({super.key});

  @override
  State<SupportTicketsScreen> createState() => _SupportTicketsScreenState();
}

class _SupportTicketsScreenState extends State<SupportTicketsScreen> {
  final _service = SupportTicketService();
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  List<SupportTicket> _tickets = [];
  SupportTicketStatus? _statusFilter;

  @override
  void initState() {
    super.initState();
    _fetchTickets();
  }

  Future<void> _fetchTickets() async {
    setState(() => _isLoading = true);
    try {
      if (_statusFilter != null) {
        final list = await _service.getByStatus(_statusFilter!);
        setState(() {
          _tickets = list;
          _page = 1;
          _totalPages = 1;
        });
      } else {
        final result = await _service.getPaged(
            pageNumber: _page, pageSize: _pageSize);
        setState(() {
          _tickets = result.items;
          _totalPages = result.totalPages;
        });
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    setState(() => _isLoading = false);
  }

  // Izvještaj mora obuhvatiti cijeli dataset, ne samo trenutno prikazanu
  // stranicu — ako je aktivan filter po statusu, taj poziv već vraća sve
  // rezultate; u suprotnom dohvati sve stranice.
  Future<List<SupportTicket>> _fetchAllTicketsForExport() async {
    if (_statusFilter != null) {
      return _service.getByStatus(_statusFilter!);
    }
    final List<SupportTicket> all = [];
    int page = 1;
    const int size = 100;
    while (true) {
      final result = await _service.getPaged(pageNumber: page, pageSize: size);
      all.addAll(result.items);
      if (all.length >= result.totalCount || result.items.isEmpty) break;
      page++;
    }
    return all;
  }

  Future<void> _exportTicketsPdf() async {
    setState(() => _isLoading = true);
    try {
      final all = await _fetchAllTicketsForExport();
      if (mounted) PdfReportService.exportSupportTickets(context, all);
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _openForm({SupportTicket? ticket}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => SupportTicketFormDialog(ticket: ticket),
    );
    if (result == true) _fetchTickets();
  }

  Future<void> _deleteTicket(int id) async {
    try {
      await _service.delete(id);
      _fetchTickets();
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Tiketi podrške'),
        actions: [
          IconButton(
            icon: const Icon(Icons.picture_as_pdf),
            tooltip: 'PDF izvještaj',
            onPressed: _exportTicketsPdf,
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                DropdownButton<SupportTicketStatus?>(
                  value: _statusFilter,
                  hint: const Text('Svi statusi'),
                  items: [
                    const DropdownMenuItem(value: null, child: Text('Svi')),
                    ...SupportTicketStatus.values.map((s) => DropdownMenuItem(
                          value: s,
                          child: Text(supportTicketStatusLabel(s)),
                        )),
                  ],
                  onChanged: (v) {
                    setState(() => _statusFilter = v);
                    _fetchTickets();
                  },
                ),
                const Spacer(),
                if (_statusFilter == null)
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.chevron_left),
                        onPressed: _page > 1
                            ? () {
                                _page--;
                                _fetchTickets();
                              }
                            : null,
                      ),
                      Text('Strana $_page / $_totalPages'),
                      IconButton(
                        icon: const Icon(Icons.chevron_right),
                        onPressed: _page < _totalPages
                            ? () {
                                _page++;
                                _fetchTickets();
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
                  : _tickets.isEmpty
                      ? const Center(child: Text('Nema tiketa.'))
                      : ListView.builder(
                          itemCount: _tickets.length,
                          itemBuilder: (context, i) {
                            final t = _tickets[i];
                            final hasResponse = (t.adminResponse ?? '').trim().isNotEmpty;
                            return Card(
                              child: ListTile(
                                title: Text('${t.subject} (${t.userName})'),
                                subtitle: Text(
                                    '${t.messageBody}\nStatus: ${supportTicketStatusLabel(t.status)}'
                                    '${hasResponse ? '\nOdgovoreno (${t.respondedByUserName ?? '-'})' : '\nBez odgovora'}'),
                                isThreeLine: true,
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.edit),
                                      onPressed: () => _openForm(ticket: t),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.delete),
                                      onPressed: () => _deleteTicket(t.id),
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
