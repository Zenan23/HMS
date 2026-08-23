import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/room.dart';
import '../models/room_maintenance_log.dart';
import '../services/api_service.dart';
import '../services/room_maintenance_log_service.dart';
import 'date_picker_field.dart';
import 'app_dialog_title.dart';
import '../utils/error_helper.dart';

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
  List<Room> _rooms = [];

  @override
  void initState() {
    super.initState();
    final l = widget.log;
    roomId = l?.roomId ?? 0;
    reportedAt = l?.reportedAt ?? DateTime.now();
    resolvedAt = l?.resolvedAt;
    description = l?.description ?? '';
    cost = l?.cost ?? 0;
    technicianName = l?.technicianName ?? '';
    _fetchRooms();
  }

  Future<void> _fetchRooms() async {
    try {
      final resp =
          await ApiService().get('/api/Rooms?pageNumber=1&pageSize=100');
      final decoded = jsonDecode(resp.body) as Map<String, dynamic>;
      final items = (decoded['data']?['items'] as List?) ?? [];
      _rooms = items.map((e) => Room.fromJson(e)).toList().cast<Room>();
    } catch (_) {}
    if (mounted) setState(() {});
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    if (resolvedAt != null && resolvedAt!.isBefore(reportedAt)) {
      setState(
          () => error = 'Datum rješavanja mora biti nakon datuma prijave.');
      return;
    }
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
      setState(() => error = friendlyErrorMessage(e));
    }
    setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: AppDialogTitle(widget.log == null ? 'Novi zapis' : 'Uredi zapis'),
      content: SizedBox(
        width: 420,
        child: SingleChildScrollView(
          child: Form(
            key: _formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                DropdownButtonFormField<int>(
                  value: _rooms.any((r) => r.id == roomId) ? roomId : null,
                  decoration: const InputDecoration(labelText: 'Soba'),
                  items: _rooms
                      .map((r) => DropdownMenuItem<int>(
                            value: r.id,
                            child: Text(
                              '${r.roomNumber}${r.hotelName != null && r.hotelName!.isNotEmpty ? ' – ${r.hotelName}' : ''}',
                            ),
                          ))
                      .toList(),
                  onChanged: (v) => setState(() => roomId = v ?? 0),
                  validator: (v) =>
                      (v == null || v == 0) ? 'Odaberite sobu' : null,
                ),
                TextFormField(
                  initialValue: description,
                  decoration: const InputDecoration(labelText: 'Opis'),
                  validator: (v) =>
                      (v == null || v.trim().isEmpty) ? 'Unesite opis' : null,
                  onChanged: (v) => description = v,
                ),
                TextFormField(
                  initialValue: cost.toString(),
                  decoration: const InputDecoration(labelText: 'Trošak (EUR)'),
                  keyboardType:
                      const TextInputType.numberWithOptions(decimal: true),
                  onChanged: (v) => cost = double.tryParse(v) ?? 0,
                ),
                TextFormField(
                  initialValue: technicianName,
                  decoration: const InputDecoration(labelText: 'Tehničar'),
                  onChanged: (v) => technicianName = v,
                ),
                const SizedBox(height: 8),
                DatePickerField(
                  label: 'Datum prijave',
                  value: reportedAt,
                  onChanged: (d) {
                    if (d != null) setState(() => reportedAt = d);
                  },
                ),
                const SizedBox(height: 8),
                DatePickerField(
                  label: 'Datum rješavanja',
                  value: resolvedAt,
                  allowClear: true,
                  onChanged: (d) => setState(() => resolvedAt = d),
                ),
                if (error != null)
                  Padding(
                    padding: const EdgeInsets.only(top: 8),
                    child: Text(error!,
                        style: const TextStyle(color: Colors.red)),
                  ),
              ],
            ),
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
