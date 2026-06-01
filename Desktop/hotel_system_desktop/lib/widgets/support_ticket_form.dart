import 'package:flutter/material.dart';
import '../models/support_ticket.dart';
import '../services/support_ticket_service.dart';

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
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final t = widget.ticket;
    userId = t?.userId ?? 1;
    subject = t?.subject ?? '';
    messageBody = t?.messageBody ?? '';
    status = t?.status ?? SupportTicketStatus.open;
    priority = t?.priority ?? SupportTicketPriority.medium;
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
              TextFormField(
                initialValue: userId.toString(),
                decoration: const InputDecoration(labelText: 'User ID'),
                keyboardType: TextInputType.number,
                onChanged: (v) => userId = int.tryParse(v) ?? userId,
                validator: (v) =>
                    int.tryParse(v ?? '') == null ? 'Obavezno' : null,
              ),
              TextFormField(
                initialValue: subject,
                decoration: const InputDecoration(labelText: 'Naslov'),
                onChanged: (v) => subject = v,
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Obavezno' : null,
              ),
              TextFormField(
                initialValue: messageBody,
                decoration: const InputDecoration(labelText: 'Poruka'),
                maxLines: 4,
                onChanged: (v) => messageBody = v,
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Obavezno' : null,
              ),
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
                          child: Text(p.name),
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
