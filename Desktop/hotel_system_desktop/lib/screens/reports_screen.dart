import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../providers/auth_provider.dart';
import '../models/booking.dart';
import '../models/hotel.dart';
import '../models/inventory_transaction.dart';
import '../models/price_adjustment.dart';
import '../models/room.dart';
import '../models/room_maintenance_log.dart';
import '../models/service.dart';
import '../models/support_ticket.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/inventory_transaction_service.dart';
import '../services/pdf_report_service.dart';
import '../services/price_adjustment_service.dart';
import '../services/room_maintenance_log_service.dart';
import '../services/support_ticket_service.dart';
import '../utils/error_helper.dart';

/// Centralizovani pregled svih PDF izvještaja (12 ukupno), na jednom mjestu
/// umjesto razbacano po ekranima — RSII uputa traži minimalno 2, aplikacija
/// ih ima 12 pa ovaj ekran samo objedinjuje pristup radi preglednosti.
/// Pojedinačni "PDF" dugmići na svakom ekranu i dalje rade (filtrirani
/// izvoz), ovaj ekran uvijek izvozi cijeli skup podataka.
class ReportsScreen extends StatefulWidget {
  const ReportsScreen({super.key});

  @override
  State<ReportsScreen> createState() => _ReportsScreenState();
}

class _ReportEntry {
  final String title;
  final String description;
  final IconData icon;
  final Future<void> Function(BuildContext context) generate;
  /// Izvještaji koji povlače podatke dostupne samo Adminu (npr. finansijska
  /// statistika/prihod preko `/api/Dashboard/statistics`, koji backend već
  /// štiti sa `[AuthorizeRole(UserRole.Admin)]`) — uposlenik ih ne treba ni
  /// vidjeti kao opciju, ne samo da mu poziv na kraju vrati 403.
  final bool adminOnly;

  _ReportEntry({
    required this.title,
    required this.description,
    required this.icon,
    required this.generate,
    this.adminOnly = false,
  });
}

class _ReportsScreenState extends State<ReportsScreen> {
  String? _loadingTitle;

  Future<List<T>> _fetchAllRaw<T>(
    String path,
    T Function(Map<String, dynamic>) fromJson,
  ) async {
    final List<T> all = [];
    int page = 1;
    const int size = 100;
    while (true) {
      final sep = path.contains('?') ? '&' : '?';
      final response = await ApiService()
          .get('$path${sep}pageNumber=$page&pageSize=$size');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      all.addAll(items.map((e) => fromJson(e as Map<String, dynamic>)));
      final int totalCount = data['totalCount'] ?? 0;
      if (all.length >= totalCount || items.isEmpty) break;
      page++;
    }
    return all;
  }

  late final List<_ReportEntry> _reports = [
    _ReportEntry(
      title: 'Hoteli',
      description: 'Lista svih hotela sa gradom, državom i ocjenom.',
      icon: Icons.hotel,
      generate: (ctx) async {
        final all = await _fetchAllRaw('/api/hotels', Hotel.fromJson);
        if (ctx.mounted) await PdfReportService.exportHotels(ctx, all);
      },
    ),
    _ReportEntry(
      title: 'Sobe',
      description: 'Lista svih soba sa tipom, cijenom i dostupnošću.',
      icon: Icons.bed,
      generate: (ctx) async {
        final all = await _fetchAllRaw('/api/Rooms', Room.fromJson);
        if (ctx.mounted) await PdfReportService.exportRooms(ctx, all);
      },
    ),
    _ReportEntry(
      title: 'Rezervacije',
      description: 'Sve rezervacije sa statusom i cijenom.',
      icon: Icons.calendar_month,
      generate: (ctx) async {
        final all = await _fetchAllRaw('/api/Bookings', Booking.fromJson);
        if (ctx.mounted) await PdfReportService.exportBookings(ctx, all);
      },
    ),
    _ReportEntry(
      title: 'Servisi',
      description: 'Svi dodatni servisi hotela.',
      icon: Icons.room_service,
      generate: (ctx) async {
        final all = await _fetchAllRaw('/api/Services', Service.fromJson);
        if (ctx.mounted) await PdfReportService.exportServices(ctx, all);
      },
    ),
    _ReportEntry(
      title: 'Uposlenici',
      description: 'Svi uposlenici (uloga Employee).',
      icon: Icons.badge,
      generate: (ctx) async {
        final response = await ApiService().get('/api/Users/role/1');
        final Map<String, dynamic> decoded = jsonDecode(response.body);
        final List data = decoded['data'] ?? [];
        final all = data.map((e) => Employee.fromJson(e)).toList();
        if (ctx.mounted) await PdfReportService.exportEmployees(ctx, all);
      },
    ),
    _ReportEntry(
      title: 'Korisnici',
      description: 'Svi registrovani korisnici sistema.',
      icon: Icons.people,
      generate: (ctx) async {
        final all = await _fetchAllRaw('/api/Users', Employee.fromJson);
        if (ctx.mounted) await PdfReportService.exportUsers(ctx, all);
      },
    ),
    _ReportEntry(
      title: 'Tiketi podrške',
      description: 'Svi tiketi podrške sa statusom i prioritetom.',
      icon: Icons.support_agent,
      generate: (ctx) async {
        final service = SupportTicketService();
        final all = <SupportTicket>[];
        int page = 1;
        const size = 100;
        while (true) {
          final result =
              await service.getPaged(pageNumber: page, pageSize: size);
          all.addAll(result.items);
          if (all.length >= result.totalCount || result.items.isEmpty) break;
          page++;
        }
        if (ctx.mounted) {
          await PdfReportService.exportSupportTickets(ctx, all);
        }
      },
    ),
    _ReportEntry(
      title: 'Prilagodbe cijena',
      description: 'Sve sezonske/promotivne prilagodbe cijena.',
      icon: Icons.price_change,
      generate: (ctx) async {
        final service = PriceAdjustmentService();
        final all = <PriceAdjustment>[];
        int page = 1;
        const size = 100;
        while (true) {
          final result =
              await service.getPaged(pageNumber: page, pageSize: size);
          all.addAll(result.items);
          if (all.length >= result.totalCount || result.items.isEmpty) break;
          page++;
        }
        if (ctx.mounted) {
          await PdfReportService.exportPriceAdjustments(ctx, all);
        }
      },
    ),
    _ReportEntry(
      title: 'Održavanje soba',
      description: 'Svi zapisi o održavanju/kvarovima soba.',
      icon: Icons.build,
      generate: (ctx) async {
        final service = RoomMaintenanceLogService();
        final all = <RoomMaintenanceLog>[];
        int page = 1;
        const size = 100;
        while (true) {
          final result =
              await service.getPaged(pageNumber: page, pageSize: size);
          all.addAll(result.items);
          if (all.length >= result.totalCount || result.items.isEmpty) break;
          page++;
        }
        if (ctx.mounted) {
          await PdfReportService.exportMaintenanceLogs(ctx, all);
        }
      },
    ),
    _ReportEntry(
      title: 'Skladišne transakcije',
      description: 'Sve transakcije artikala skladišta.',
      icon: Icons.inventory,
      generate: (ctx) async {
        final service = InventoryTransactionService();
        final all = <InventoryTransaction>[];
        int page = 1;
        const size = 100;
        while (true) {
          final result =
              await service.getPaged(pageNumber: page, pageSize: size);
          all.addAll(result.items);
          if (all.length >= result.totalCount || result.items.isEmpty) break;
          page++;
        }
        if (ctx.mounted) {
          await PdfReportService.exportInventoryTransactions(ctx, all);
        }
      },
    ),
    _ReportEntry(
      title: 'Statistika (Pregled)',
      description: 'Sažeti pregled poslovanja — prihod, popunjenost, top hoteli.',
      icon: Icons.dashboard,
      adminOnly: true,
      generate: (ctx) async {
        final stats = await ApiService().getDashboardStatistics();
        if (ctx.mounted) {
          await PdfReportService.exportDashboard(ctx, stats);
        }
      },
    ),
  ];

  Future<void> _generate(_ReportEntry entry) async {
    setState(() => _loadingTitle = entry.title);
    try {
      await entry.generate(context);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Greška pri generisanju izvještaja: ${friendlyErrorMessage(e)}')),
        );
      }
    }
    if (mounted) setState(() => _loadingTitle = null);
  }

  @override
  Widget build(BuildContext context) {
    final isAdmin = Provider.of<AuthProvider>(context, listen: false).isAdmin;
    final visibleReports =
        isAdmin ? _reports : _reports.where((r) => !r.adminOnly).toList();
    return Scaffold(
      appBar: AppBar(title: const Text('Izvještaji')),
      body: GridView.builder(
        padding: const EdgeInsets.all(16),
        gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(
          maxCrossAxisExtent: 340,
          mainAxisExtent: 150,
          crossAxisSpacing: 12,
          mainAxisSpacing: 12,
        ),
        itemCount: visibleReports.length,
        itemBuilder: (context, i) {
          final entry = visibleReports[i];
          final isLoading = _loadingTitle == entry.title;
          return Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Icon(entry.icon),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(entry.title,
                            style: const TextStyle(
                                fontWeight: FontWeight.bold, fontSize: 16)),
                      ),
                    ],
                  ),
                  const SizedBox(height: 6),
                  Expanded(
                    child: Text(entry.description,
                        style: const TextStyle(fontSize: 12)),
                  ),
                  Align(
                    alignment: Alignment.centerRight,
                    child: ElevatedButton.icon(
                      icon: isLoading
                          ? const SizedBox(
                              width: 14,
                              height: 14,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2))
                          : const Icon(Icons.picture_as_pdf, size: 16),
                      label: const Text('PDF'),
                      onPressed:
                          _loadingTitle == null ? () => _generate(entry) : null,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
