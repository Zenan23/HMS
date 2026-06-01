import 'package:flutter/material.dart';
import '../models/room_maintenance_log.dart';
import '../services/room_maintenance_log_service.dart';
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
  int _pageSize = 10;
  int _totalPages = 1;
  bool _isLoading = false;
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
          _page = 1;
          _totalPages = 1;
        });
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        setState(() {
          _logs = result.items;
          _totalPages = result.totalPages;
        });
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    setState(() => _isLoading = false);
  }

  Future<void> _openForm({RoomMaintenanceLog? log}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => RoomMaintenanceLogFormDialog(log: log),
    );
    if (result == true) _fetchLogs();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Održavanje soba')),
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
                SizedBox(
                  width: 120,
                  child: TextField(
                    controller: _roomIdController,
                    decoration: const InputDecoration(labelText: 'Room ID'),
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
                      _fetchLogs();
                    }
                  },
                  child: const Text('Filtriraj'),
                ),
              ],
            ),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : ListView.builder(
                      itemCount: _logs.length,
                      itemBuilder: (context, i) {
                        final log = _logs[i];
                        return Card(
                          child: ListTile(
                            title: Text('Soba ${log.roomId} - ${log.description}'),
                            subtitle: Text(
                                'Prijavljeno: ${log.reportedAt}\nTehničar: ${log.technicianName}\nTrošak: ${log.cost} EUR'),
                            isThreeLine: true,
                            trailing: IconButton(
                              icon: const Icon(Icons.edit),
                              onPressed: () => _openForm(log: log),
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
