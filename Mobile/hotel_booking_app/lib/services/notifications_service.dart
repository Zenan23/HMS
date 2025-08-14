import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:signalr_netcore/signalr_client.dart';
import 'api_service.dart';
import 'auth_service.dart';

class NotificationsService extends ChangeNotifier {
  HubConnection? _hub;
  int _unreadCount = 0;

  int get unreadCount => _unreadCount;

  Future<void> init(BuildContext context) async {
    final auth = context.read<AuthService>();
    final token = auth.token;
    if (token == null) return;

    // Izvuci origin iz API base URL-a (bez "/api") i složi ws/wss URL za SignalR hub
    final apiUri = Uri.parse(ApiService.baseUrl);
    final isSecure = apiUri.scheme == 'https';
    // SignalR negotiate MORA ići preko http/https; library će sam preći na WebSocket
    final hubUri = Uri(
      scheme: isSecure ? 'https' : 'http',
      host: apiUri.host,
      port: apiUri.hasPort ? apiUri.port : null,
      path: '/hubs/notifications',
      queryParameters: {'access_token': token},
    );
    final hubUrl = hubUri.toString();

    _hub = HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    _hub!.on('notificationCreated', (args) async {
      // args[0] expected: { notificationId: n }
      await refreshUnread(context);
      notifyListeners();
    });

    await _hub!.start();

    await refreshUnread(context);
  }

  Future<void> refreshUnread(BuildContext context) async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId == null) return;
    try {
      final resp = await ApiService.get('/Notifications/unread-count/$userId');
      if (resp.statusCode == 200) {
        final data = jsonDecode(resp.body);
        _unreadCount = (data['data'] as num?)?.toInt() ?? 0;
      }
    } catch (_) {}
    notifyListeners();
  }

  Future<void> disposeHub() async {
    try { await _hub?.stop(); } catch (_) {}
    _hub = null;
  }
}


