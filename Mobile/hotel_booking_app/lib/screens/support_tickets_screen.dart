import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/support_ticket.dart';
import '../services/auth_service.dart';
import '../services/support_tickets_service.dart';
import '../utils/api_response.dart';

class SupportTicketsScreen extends StatefulWidget {
  const SupportTicketsScreen({super.key});

  @override
  State<SupportTicketsScreen> createState() => _SupportTicketsScreenState();
}

class _SupportTicketsScreenState extends State<SupportTicketsScreen> {
  final _service = SupportTicketsService();
  final _subjectController = TextEditingController();
  final _messageController = TextEditingController();
  List<SupportTicket> _tickets = [];
  bool _loading = true;
  String? _error;
  SupportTicketPriority _priority = SupportTicketPriority.medium;

  @override
  void initState() {
    super.initState();
    _loadTickets();
  }

  @override
  void dispose() {
    _subjectController.dispose();
    _messageController.dispose();
    super.dispose();
  }

  Future<void> _loadTickets() async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId == null) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final tickets = await _service.getByUserId(userId);
      setState(() => _tickets = tickets);
    } on ApiException catch (e) {
      setState(() => _error = e.message);
    } catch (_) {
      setState(() => _error = 'Greška pri učitavanju tiketa.');
    } finally {
      setState(() => _loading = false);
    }
  }

  Future<void> _createTicket() async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId == null) return;
    if (_subjectController.text.trim().isEmpty ||
        _messageController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Unesite naslov i poruku.')),
      );
      return;
    }
    try {
      await _service.create(
        userId: userId,
        subject: _subjectController.text.trim(),
        messageBody: _messageController.text.trim(),
        priority: _priority,
      );
      _subjectController.clear();
      _messageController.clear();
      if (mounted) Navigator.pop(context);
      await _loadTickets();
    } on ApiException catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text(e.message)));
      }
    }
  }

  void _openCreateDialog() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Novi tiket'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: _subjectController,
                decoration: const InputDecoration(labelText: 'Naslov'),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: _messageController,
                decoration: const InputDecoration(labelText: 'Poruka'),
                maxLines: 4,
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<SupportTicketPriority>(
                value: _priority,
                decoration: const InputDecoration(labelText: 'Prioritet'),
                items: SupportTicketPriority.values
                    .map((p) => DropdownMenuItem(
                          value: p,
                          child: Text(p.name),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => _priority = v ?? _priority),
              ),
            ],
          ),
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Odustani')),
          ElevatedButton(onPressed: _createTicket, child: const Text('Pošalji')),
        ],
      ),
    );
  }

  String _statusLabel(SupportTicketStatus status) {
    switch (status) {
      case SupportTicketStatus.open:
        return 'Otvoren';
      case SupportTicketStatus.inProgress:
        return 'U toku';
      case SupportTicketStatus.closed:
        return 'Zatvoren';
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Podrška')),
      floatingActionButton: FloatingActionButton(
        onPressed: _openCreateDialog,
        child: const Icon(Icons.add),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Text(_error!))
              : _tickets.isEmpty
                  ? const Center(child: Text('Nemate tiketa podrške.'))
                  : RefreshIndicator(
                      onRefresh: _loadTickets,
                      child: ListView.builder(
                        itemCount: _tickets.length,
                        itemBuilder: (context, i) {
                          final t = _tickets[i];
                          final hasResponse =
                              (t.adminResponse ?? '').trim().isNotEmpty;
                          return Card(
                            margin: const EdgeInsets.symmetric(
                                horizontal: 16, vertical: 8),
                            child: Padding(
                              padding: const EdgeInsets.all(12),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(t.subject,
                                      style: const TextStyle(
                                          fontWeight: FontWeight.bold)),
                                  const SizedBox(height: 4),
                                  Text(t.messageBody),
                                  const SizedBox(height: 4),
                                  Text('Status: ${_statusLabel(t.status)}',
                                      style: Theme.of(context)
                                          .textTheme
                                          .bodySmall),
                                  if (hasResponse) ...[
                                    const SizedBox(height: 10),
                                    Container(
                                      width: double.infinity,
                                      padding: const EdgeInsets.all(10),
                                      decoration: BoxDecoration(
                                        color: Theme.of(context)
                                            .colorScheme
                                            .primaryContainer
                                            .withOpacity(0.4),
                                        borderRadius:
                                            BorderRadius.circular(8),
                                      ),
                                      child: Column(
                                        crossAxisAlignment:
                                            CrossAxisAlignment.start,
                                        children: [
                                          Text(
                                            'Odgovor${t.respondedByUserName != null ? ' — ${t.respondedByUserName}' : ''}',
                                            style: const TextStyle(
                                                fontWeight: FontWeight.w600,
                                                fontSize: 12),
                                          ),
                                          const SizedBox(height: 4),
                                          Text(t.adminResponse!.trim()),
                                        ],
                                      ),
                                    ),
                                  ] else ...[
                                    const SizedBox(height: 6),
                                    Text(
                                      'Čeka se odgovor podrške.',
                                      style: TextStyle(
                                          fontSize: 12,
                                          fontStyle: FontStyle.italic,
                                          color: Theme.of(context)
                                              .colorScheme
                                              .outline),
                                    ),
                                  ],
                                ],
                              ),
                            ),
                          );
                        },
                      ),
                    ),
    );
  }
}
