import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/support_ticket.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/support_ticket_service.dart';
import '../utils/display_labels.dart';

class SupportTicketFormDialog extends StatefulWidget {
  final SupportTicket? ticket;
  const SupportTicketFormDialog({super.key, this.ticket});

  @override
  State<SupportTicketFormDialog> createState() =>
      _SupportTicketFormDialogState();
}

class _SupportTicketFormDialogState extends State<SupportTicketFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = SupportTicketService();
  late int userId;
  late String subject;
  late String messageBody;
  late SupportTicketStatus status;
  late SupportTicketPriority priority;
  late TextEditingController _adminResponseController;
  bool isLoading = false;
  String? error;
  List<Employee> _users = [];

  bool get _isEditingExisting => widget.ticket != null;

  @override
  void initState() {
    super.initState();
    final t = widget.ticket;
    userId = t?.userId ?? 0;
    subject = t?.subject ?? '';
    messageBody = t?.messageBody ?? '';
    status = t?.status ?? SupportTicketStatus.open;
    priority = t?.priority ?? SupportTicketPriority.medium;
    _adminResponseController = TextEditingController(text: t?.adminResponse ?? '');
    _fetchUsers();
  }

  @override
  void dispose() {
    _adminResponseController.dispose();
    super.dispose();
  }

  Future<void> _fetchUsers() async {
    try {
      // Gosti (0) + uposlenici (1) — tiketi mogu biti od oba tipa
      final guestsResp = await ApiService().get('/api/Users/role/0');
      final employeesResp = await ApiService().get('/api/Users/role/1');
      final guestsDecoded = jsonDecode(guestsResp.body);
      final employeesDecoded = jsonDecode(employeesResp.body);
      final List guestItems = (guestsDecoded['data'] ?? []) as List;
      final List employeeItems = (employeesDecoded['data'] ?? []) as List;
      _users = [
        ...guestItems.map((e) => Employee.fromJson(e)),
        ...employeeItems.map((e) => Employee.fromJson(e)),
      ];
    } catch (_) {}
    if (mounted) setState(() {});
  }

  String _userLabel(Employee u) {
    final name = u.fullName.isNotEmpty ? u.fullName : u.username;
    return '$name (${u.email})';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.ticket?.id ?? 0,
      'userId': userId,
      'subject': subject,
      'messageBody': messageBody,
      'status': supportTicketStatusToInt(status),
      'priority': supportTicketPriorityToInt(priority),
      if (_isEditingExisting) 'adminResponse': _adminResponseController.text,
    };
    try {
      if (widget.ticket == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.ticket!.id, body);
      }
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      setState(() => error = e.toString());
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(widget.ticket == null ? 'Novi tiket' : 'Uredi tiket'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              DropdownButtonFormField<int>(
                value: _users.any((u) => u.id == userId) ? userId : null,
                decoration: const InputDecoration(labelText: 'Korisnik'),
                items: _users
                    .map((u) => DropdownMenuItem<int>(
                          value: u.id,
                          child: Text(_userLabel(u)),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => userId = v ?? 0),
                validator: (v) =>
                    (v == null || v == 0) ? 'Odaberite korisnika' : null,
              ),
              TextFormField(
                initialValue: subject,
                decoration: const InputDecoration(labelText: 'Naslov'),
                // Gostov originalni naslov/poruka se ne mijenjaju kroz "odgovori na tiket" tok —
                // uređivanje ostaje moguće samo kad zaposlenik ručno kreira/ispravlja tiket.
                readOnly: _isEditingExisting,
                onChanged: (v) => subject = v,
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Obavezno' : null,
              ),
              TextFormField(
                initialValue: messageBody,
                decoration: const InputDecoration(labelText: 'Poruka (gost)'),
                maxLines: 4,
                readOnly: _isEditingExisting,
                onChanged: (v) => messageBody = v,
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Obavezno' : null,
              ),
              if (_isEditingExisting) ...[
                const SizedBox(height: 12),
                if (widget.ticket?.respondedAt != null)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 8),
                    child: Text(
                      'Prethodno odgovorio: ${widget.ticket?.respondedByUserName ?? '-'} · '
                      '${widget.ticket!.respondedAt!.toLocal().toString().split('.').first}',
                      style: const TextStyle(
                          fontStyle: FontStyle.italic, fontSize: 12),
                    ),
                  ),
                TextFormField(
                  controller: _adminResponseController,
                  decoration: const InputDecoration(
                    labelText: 'Odgovor gostu',
                    helperText:
                        'Gost dobija notifikaciju kad ovdje upišete/promijenite odgovor.',
                  ),
                  maxLines: 4,
                ),
                const SizedBox(height: 8),
              ],
              DropdownButtonFormField<SupportTicketStatus>(
                value: status,
                decoration: const InputDecoration(labelText: 'Status'),
                items: SupportTicketStatus.values
                    .map((s) => DropdownMenuItem(
                          value: s,
                          child: Text(supportTicketStatusLabel(s)),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => status = v ?? status),
              ),
              DropdownButtonFormField<SupportTicketPriority>(
                value: priority,
                decoration: const InputDecoration(labelText: 'Prioritet'),
                items: SupportTicketPriority.values
                    .map((p) => DropdownMenuItem(
                          value: p,
                          child: Text(supportTicketPriorityLabel(p)),
                        ))
                    .toList(),
                onChanged: (v) => setState(() => priority = v ?? priority),
              ),
              if (error != null)
                Text(error!, style: const TextStyle(color: Colors.red)),
            ],
          ),
        ),
      ),
      actions: [
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
