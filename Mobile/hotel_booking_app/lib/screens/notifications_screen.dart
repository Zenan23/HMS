import 'package:flutter/material.dart';
import 'dart:convert';
import 'package:provider/provider.dart';
import '../services/api_service.dart';
import '../services/auth_service.dart';
import '../services/notifications_service.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({Key? key}) : super(key: key);

  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  bool _loading = false;
  String? _error;
  List<Map<String, dynamic>> _items = [];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _load());
  }

  Future<void> _load() async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId == null) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final resp = await ApiService.get('/Notifications/user/$userId');
      if (resp.statusCode == 200) {
        final decoded = jsonDecode(resp.body);
        final List data = decoded['data'] ?? [];
        setState(() {
          _items = data.cast<Map<String, dynamic>>();
        });
        // osvježi unread badge
        await context.read<NotificationsService>().refreshUnread(context);
      } else {
        setState(() {
          _error = 'Greška pri dohvatu notifikacija';
        });
      }
    } catch (_) {
      setState(() {
        _error = 'Greška pri povezivanju sa serverom';
      });
    } finally {
      if (mounted)
        setState(() {
          _loading = false;
        });
    }
  }

  Future<void> _markRead(int id) async {
    try {
      await ApiService.patch('/Notifications/$id/mark-read', {'id': id});
      await _load();
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Notifikacije')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? Center(child: Text(_error!))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    separatorBuilder: (_, __) => const Divider(height: 1),
                    itemCount: _items.length,
                    itemBuilder: (context, i) {
                      final n = _items[i];
                      final isRead = n['isRead'] == true;
                      return ListTile(
                        leading: Icon(
                            isRead
                                ? Icons.notifications_none
                                : Icons.notifications_active,
                            color: isRead ? Colors.grey : Colors.blue),
                        title: Text(n['title'] ?? ''),
                        subtitle: Text(n['message'] ?? ''),
                        trailing: isRead
                            ? null
                            : TextButton(
                                onPressed: () => _markRead(n['id'] as int),
                                child: const Text('Označi pročitano'),
                              ),
                      );
                    },
                  ),
                ),
    );
  }
}
