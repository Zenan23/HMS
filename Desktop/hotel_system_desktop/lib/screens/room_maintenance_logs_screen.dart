import 'package:flutter/material.dart';
import '../models/room_maintenance_log.dart';
import '../services/room_maintenance_log_service.dart';
import '../utils/date_format_utils.dart';
import '../utils/error_helper.dart';
import '../widgets/room_maintenance_log_form.dart';

class RoomMaintenanceLogsScreen extends StatefulWidget {
  const RoomMaintenanceLogsScreen({super.key});

  @override
  State<RoomMaintenanceLogsScreen> createState() =>
      _RoomMaintenanceLogsScreenState();
}

class _RoomMaintenanceLogsScreenState extends State<RoomMaintenanceLogsScreen> {
  final _service = RoomMaintenanceLogService();
  int _page = 1;
  final int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
  bool _filterByRoom = false;
  List<RoomMaintenanceLog> _logs = [];
  final _roomIdController = TextEditingController();

  @override
  void initState() {
    super.initState();
    _fetchLogs();
  }

  @override
  void dispose() {
    _roomIdController.dispose();
    super.dispose();
  }

  Future<void> _fetchLogs({int? roomId}) async {
    setState(() => _isLoading = true);
    try {
      if (roomId != null) {
        final list = await _service.getByRoomId(roomId);
        setState(() {
          _logs = list;
          _filterByRoom = true;
          _page = 1;
          _totalPages = 1;
        });
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        setState(() {
          _logs = result.items;
          _filterByRoom = false;
          _totalPages = result.totalPages < 1 ? 1 : result.totalPages;
        });
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _openForm({RoomMaintenanceLog? log}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => RoomMaintenanceLogFormDialog(log: log),
    );
    if (result == true) {
      final roomId = int.tryParse(_roomIdController.text);
      if (_filterByRoom && roomId != null) {
        _fetchLogs(roomId: roomId);
      } else {
        _fetchLogs();
      }
    }
  }

  Future<void> _delete(RoomMaintenanceLog log) async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Potvrda brisanja'),
        content: Text('Obrisati zapis za sobu ${log.roomId}?'),
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
      await _service.delete(log.id);
      final roomId = int.tryParse(_roomIdController.text);
      if (_filterByRoom && roomId != null) {
        _fetchLogs(roomId: roomId);
      } else {
        _fetchLogs();
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Održavanje soba')),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        tooltip: 'Novi zapis',
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                SizedBox(
                  width: 140,
                  child: TextField(
                    controller: _roomIdController,
                    decoration: const InputDecoration(labelText: 'ID sobe'),
                    keyboardType: TextInputType.number,
                  ),
                ),
                const SizedBox(width: 8),
                ElevatedButton(
                  onPressed: () {
                    final roomId = int.tryParse(_roomIdController.text);
                    if (roomId != null) {
                      _fetchLogs(roomId: roomId);
                    } else {
                      setState(() => _page = 1);
                      _fetchLogs();
                    }
                  },
                  child: const Text('Filtriraj'),
                ),
                const SizedBox(width: 8),
                TextButton(
                  onPressed: () {
                    _roomIdController.clear();
                    setState(() => _page = 1);
                    _fetchLogs();
                  },
                  child: const Text('Očisti'),
                ),
                const Spacer(),
                if (!_filterByRoom)
                  Row(
                    children: [
                      IconButton(
                        icon: const Icon(Icons.chevron_left),
                        onPressed: _page > 1 && !_isLoading
                            ? () {
                                setState(() => _page--);
                                _fetchLogs();
                              }
                            : null,
                      ),
                      Text('Strana $_page / $_totalPages'),
                      IconButton(
                        icon: const Icon(Icons.chevron_right),
                        onPressed: _page < _totalPages && !_isLoading
                            ? () {
                                setState(() => _page++);
                                _fetchLogs();
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
                  : _logs.isEmpty
                      ? const Center(child: Text('Nema zapisa održavanja.'))
                      : ListView.builder(
                          itemCount: _logs.length,
                          itemBuilder: (context, i) {
                            final log = _logs[i];
                            final resolved = log.resolvedAt != null
                                ? formatDisplayDate(log.resolvedAt)
                                : 'Nije riješeno';
                            return Card(
                              child: ListTile(
                                title: Text(
                                    'Soba ${log.roomId} – ${log.description}'),
                                subtitle: Text(
                                  'Prijavljeno: ${formatDisplayDate(log.reportedAt)}\n'
                                  'Riješeno: $resolved\n'
                                  'Tehničar: ${log.technicianName} • Trošak: ${log.cost.toStringAsFixed(2)} EUR',
                                ),
                                isThreeLine: true,
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.edit),
                                      tooltip: 'Uredi',
                                      onPressed: () => _openForm(log: log),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.delete),
                                      tooltip: 'Obriši',
                                      onPressed: () => _delete(log),
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
