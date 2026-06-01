import 'package:flutter/material.dart';
import '../models/room_maintenance_log.dart';
import '../services/room_maintenance_log_service.dart';

class RoomMaintenanceLogFormDialog extends StatefulWidget {
  final RoomMaintenanceLog? log;
  const RoomMaintenanceLogFormDialog({super.key, this.log});

  @override
  State<RoomMaintenanceLogFormDialog> createState() =>
      _RoomMaintenanceLogFormDialogState();
}

class _RoomMaintenanceLogFormDialogState
    extends State<RoomMaintenanceLogFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _service = RoomMaintenanceLogService();
  late int roomId;
  late DateTime reportedAt;
  DateTime? resolvedAt;
  late String description;
  late double cost;
  late String technicianName;
  bool isLoading = false;
  String? error;

  @override
  void initState() {
    super.initState();
    final l = widget.log;
    roomId = l?.roomId ?? 1;
    reportedAt = l?.reportedAt ?? DateTime.now();
    resolvedAt = l?.resolvedAt;
    description = l?.description ?? '';
    cost = l?.cost ?? 0;
    technicianName = l?.technicianName ?? '';
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() {
      isLoading = true;
      error = null;
    });
    final body = {
      'id': widget.log?.id ?? 0,
      'roomId': roomId,
      'reportedAt': reportedAt.toIso8601String(),
      'resolvedAt': resolvedAt?.toIso8601String(),
      'description': description,
      'cost': cost,
      'technicianName': technicianName,
    };
    try {
      if (widget.log == null) {
        await _service.create(body);
      } else {
        await _service.update(widget.log!.id, body);
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
      title: Text(widget.log == null ? 'Novi zapis' : 'Uredi zapis'),
      content: SingleChildScrollView(
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                initialValue: roomId.toString(),
                decoration: const InputDecoration(labelText: 'Room ID'),
                keyboardType: TextInputType.number,
                onChanged: (v) => roomId = int.tryParse(v) ?? roomId,
              ),
              TextFormField(
                initialValue: description,
                decoration: const InputDecoration(labelText: 'Opis'),
                onChanged: (v) => description = v,
              ),
              TextFormField(
                initialValue: cost.toString(),
                decoration: const InputDecoration(labelText: 'Trošak'),
                keyboardType:
                    const TextInputType.numberWithOptions(decimal: true),
                onChanged: (v) => cost = double.tryParse(v) ?? 0,
              ),
              TextFormField(
                initialValue: technicianName,
                decoration: const InputDecoration(labelText: 'Tehničar'),
                onChanged: (v) => technicianName = v,
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
