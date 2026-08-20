import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../services/reservations_service.dart';
import '../services/payments_service.dart';
import '../services/loyalty_points_redemptions_service.dart';
import '../models/loyalty_points_redemption.dart';
import '../services/auth_service.dart';
import '../models/reservation.dart';
import '../utils/api_response.dart';
import '../widgets/reservation_detail_sheet.dart';
import 'payment_screen.dart';

final _dateFormat = DateFormat('dd.MM.yyyy');

class ReservationsScreen extends StatefulWidget {
  const ReservationsScreen({super.key});

  @override
  State<ReservationsScreen> createState() => _ReservationsScreenState();
}

class _ReservationsScreenState extends State<ReservationsScreen> {
  Future<List<Reservation>>? _paidFuture;
  Future<List<Reservation>>? _unpaidFuture;
  List<LoyaltyPointsRedemption> _loyaltyForBookings = [];
  final Set<int> _processingIds = {};

  Future<void> _loadData(int userId) async {
    final service = ReservationsService();
    final paidFuture = service.fetchPaidReservations(userId);
    final unpaidFuture = service.fetchUnpaidReservations(userId);
    final loyaltyService = LoyaltyPointsRedemptionsService();
    final allRedemptions = await loyaltyService.getByUserId(userId);
    final paid = await paidFuture;
    final unpaid = await unpaidFuture;
    setState(() {
      _paidFuture = Future.value(paid);
      _unpaidFuture = Future.value(unpaid);
      _loyaltyForBookings = allRedemptions;
    });
  }

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final auth = context.read<AuthService>();
      final userId = auth.user?.userId;
      if (userId != null) {
        _loadData(userId);
      }
    });
  }

  void _openDetails(Reservation reservation) {
    final loyaltyMatches =
        _loyaltyForBookings.where((l) => l.bookingId == reservation.id);
    showReservationDetailSheet(
      context,
      reservation: reservation,
      loyalty: loyaltyMatches.isNotEmpty ? loyaltyMatches.first : null,
    );
  }

  void _payAgain(Reservation reservation) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => PaymentScreen(
          bookingId: reservation.id,
          amount: reservation.totalPrice.toDouble(),
          currency: 'EUR',
        ),
      ),
    );
  }

  /// Otkazivanje rezervacije — uz potvrdni dijalog (nepovratna akcija). Za plaćene rezervacije
  /// prvo pronalazi plaćanje koje se može refundirati i traži povrat novca, pa tek onda otkazuje
  /// rezervaciju na backendu.
  Future<void> _confirmAndCancel(Reservation r, {required bool paid}) async {
    final reasonController = TextEditingController();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(paid ? 'Otkazivanje i povrat novca' : 'Otkazivanje rezervacije'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              paid
                  ? 'Rezervacija #${r.id} će biti otkazana, a iznos od ${r.totalPrice.toStringAsFixed(2)} EUR biće vraćen na način plaćanja koji ste koristili. Ova akcija se ne može poništiti.'
                  : 'Rezervacija #${r.id} će biti otkazana. Ova akcija se ne može poništiti.',
            ),
            const SizedBox(height: 16),
            TextField(
              controller: reasonController,
              decoration: const InputDecoration(
                labelText: 'Razlog otkazivanja (opcionalno)',
                border: OutlineInputBorder(),
              ),
              maxLines: 2,
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.of(ctx).pop(true),
            child: Text(paid ? 'Otkaži i vrati novac' : 'Otkaži rezervaciju'),
          ),
        ],
      ),
    );

    if (confirmed != true || !mounted) return;

    setState(() => _processingIds.add(r.id));
    try {
      final reason = reasonController.text.trim();
      if (paid) {
        final paymentsService = PaymentsService();
        final payments = await paymentsService.getPaymentsByBooking(r.id);
        final refundable = payments.where((p) => p.isRefundable).toList();
        if (refundable.isEmpty) {
          throw ApiException(
              'Nije pronađeno plaćanje koje se može refundirati za ovu rezervaciju.');
        }
        final payment = refundable.first;
        await paymentsService.refundPayment(
          payment.id,
          payment.amount,
          reason.isEmpty ? 'Otkazivanje rezervacije od strane korisnika' : reason,
        );
      }

      final reservationsService = ReservationsService();
      await reservationsService.cancelReservation(
        r.id,
        reason: reason.isEmpty ? null : reason,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(paid
              ? 'Rezervacija je otkazana, povrat novca je pokrenut.'
              : 'Rezervacija je otkazana.'),
          backgroundColor: Colors.green,
        ),
      );
      final userId = context.read<AuthService>().user?.userId;
      if (userId != null) await _loadData(userId);
    } catch (e) {
      if (!mounted) return;
      final message = e is ApiException ? e.message : 'Greška pri otkazivanju rezervacije.';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(message), backgroundColor: Colors.red),
      );
    } finally {
      if (mounted) setState(() => _processingIds.remove(r.id));
    }
  }

  @override
  Widget build(BuildContext context) {
    final userId = context.watch<AuthService>().user?.userId;
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Moje rezervacije'),
          bottom: const TabBar(
            tabs: [
              Tab(text: 'Plaćene'),
              Tab(text: 'Na čekanju'),
            ],
          ),
        ),
        body: userId == null
            ? const Center(child: Text('Niste prijavljeni.'))
            : TabBarView(
                children: [
                  _ReservationsList(
                    future: _paidFuture,
                    loyaltyForBookings: _loyaltyForBookings,
                    onRefresh: () => _loadData(userId),
                    onOpenDetails: _openDetails,
                    onCancel: (r) => _confirmAndCancel(r, paid: true),
                    processingIds: _processingIds,
                    emptyMessage: 'Nemate plaćenih rezervacija.',
                    paid: true,
                  ),
                  _ReservationsList(
                    future: _unpaidFuture,
                    loyaltyForBookings: _loyaltyForBookings,
                    onRefresh: () => _loadData(userId),
                    onOpenDetails: _openDetails,
                    onPayAgain: _payAgain,
                    onCancel: (r) => _confirmAndCancel(r, paid: false),
                    processingIds: _processingIds,
                    emptyMessage: 'Nemate rezervacija na čekanju plaćanja.',
                    paid: false,
                  ),
                ],
              ),
      ),
    );
  }
}

class _ReservationsList extends StatelessWidget {
  final Future<List<Reservation>>? future;
  final List<LoyaltyPointsRedemption> loyaltyForBookings;
  final Future<void> Function() onRefresh;
  final void Function(Reservation) onOpenDetails;
  final void Function(Reservation)? onPayAgain;
  final void Function(Reservation)? onCancel;
  final Set<int> processingIds;
  final String emptyMessage;
  final bool paid;

  const _ReservationsList({
    required this.future,
    required this.loyaltyForBookings,
    required this.onRefresh,
    required this.onOpenDetails,
    required this.emptyMessage,
    required this.paid,
    this.onPayAgain,
    this.onCancel,
    this.processingIds = const {},
  });

  @override
  Widget build(BuildContext context) {
    if (future == null) {
      return const Center(child: CircularProgressIndicator());
    }
    return FutureBuilder<List<Reservation>>(
      future: future,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }
        if (snapshot.hasError) {
          return const Center(child: Text('Greška pri dohvatu rezervacija.'));
        }
        final reservations = snapshot.data ?? [];
        if (reservations.isEmpty) {
          return RefreshIndicator(
            onRefresh: onRefresh,
            child: ListView(
              children: [
                SizedBox(
                  height: MediaQuery.of(context).size.height * 0.6,
                  child: Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        const Icon(Icons.info_outline,
                            size: 64, color: Colors.grey),
                        const SizedBox(height: 16),
                        Text(
                          emptyMessage,
                          style: const TextStyle(
                              fontSize: 18, color: Colors.grey),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          );
        }
        return RefreshIndicator(
          onRefresh: onRefresh,
          child: ListView.builder(
            padding: const EdgeInsets.symmetric(vertical: 8),
            itemCount: reservations.length,
            itemBuilder: (context, i) {
              final r = reservations[i];
              final loyalty = loyaltyForBookings
                  .where((l) => l.bookingId == r.id)
                  .toList();
              final serviceCount = r.services.length;
              // Status 5 = Otkazana, 6 = No-show — plaćanje se ne nudi za njih.
              final canPayAgain =
                  !paid && onPayAgain != null && r.status != 5 && r.status != 6;
              // Otkazivanje ima smisla samo dok rezervacija čeka (1) ili je potvrđena (2) —
              // nakon check-in/check-out/otkazivanja/no-show akcija više nije relevantna.
              final canCancel =
                  onCancel != null && (r.status == 1 || r.status == 2);
              final isProcessing = processingIds.contains(r.id);

              return Card(
                color: paid ? Colors.green.shade50 : Colors.orange.shade50,
                margin:
                    const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: InkWell(
                  borderRadius: BorderRadius.circular(12),
                  onTap: () => onOpenDetails(r),
                  child: Padding(
                    padding: const EdgeInsets.all(16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            CircleAvatar(
                              backgroundColor: paid
                                  ? Colors.green.shade100
                                  : Colors.orange.shade100,
                              child: Icon(
                                paid
                                    ? Icons.check_circle
                                    : Icons.hourglass_bottom,
                                color: paid ? Colors.green : Colors.orange.shade800,
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text(
                                    'Rezervacija #${r.id}',
                                    style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 16,
                                    ),
                                  ),
                                  Text(
                                    paid ? 'Plaćeno' : r.statusLabel,
                                    style: TextStyle(
                                      color: paid
                                          ? Colors.green.shade700
                                          : Colors.orange.shade800,
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                            Icon(Icons.chevron_right,
                                color: Colors.grey.shade600),
                          ],
                        ),
                        const SizedBox(height: 12),
                        _InfoChip(
                          icon: Icons.calendar_today,
                          text:
                              '${_formatDate(r.checkInDate)} – ${_formatDate(r.checkOutDate)}',
                        ),
                        const SizedBox(height: 6),
                        _InfoChip(
                          icon: Icons.people_outline,
                          text: '${r.numberOfGuests} gostiju',
                        ),
                        const SizedBox(height: 6),
                        _InfoChip(
                          icon: Icons.payments_outlined,
                          text: '${r.totalPrice.toStringAsFixed(2)} EUR',
                        ),
                        if (serviceCount > 0) ...[
                          const SizedBox(height: 6),
                          _InfoChip(
                            icon: Icons.room_service_outlined,
                            text: '$serviceCount dodatne usluge',
                          ),
                        ],
                        if (loyalty.isNotEmpty) ...[
                          const SizedBox(height: 6),
                          _InfoChip(
                            icon: Icons.stars,
                            text: 'Loyalty: ${loyalty.first.pointsUsed} bodova',
                            iconColor: Colors.amber.shade800,
                          ),
                        ],
                        if (canPayAgain) ...[
                          const SizedBox(height: 12),
                          SizedBox(
                            width: double.infinity,
                            child: ElevatedButton.icon(
                              icon: const Icon(Icons.replay),
                              label: const Text('Plati ponovo'),
                              onPressed: isProcessing ? null : () => onPayAgain!(r),
                            ),
                          ),
                        ],
                        if (canCancel) ...[
                          const SizedBox(height: 8),
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton.icon(
                              style: OutlinedButton.styleFrom(
                                foregroundColor: Colors.red,
                                side: const BorderSide(color: Colors.red),
                              ),
                              icon: isProcessing
                                  ? const SizedBox(
                                      width: 16,
                                      height: 16,
                                      child: CircularProgressIndicator(
                                          strokeWidth: 2, color: Colors.red),
                                    )
                                  : const Icon(Icons.cancel_outlined),
                              label: Text(paid
                                  ? 'Otkaži i zatraži povrat'
                                  : 'Otkaži rezervaciju'),
                              onPressed:
                                  isProcessing ? null : () => onCancel!(r),
                            ),
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              );
            },
          ),
        );
      },
    );
  }

  String _formatDate(DateTime? value) {
    if (value == null) return '-';
    return _dateFormat.format(value.toLocal());
  }
}

class _InfoChip extends StatelessWidget {
  final IconData icon;
  final String text;
  final Color? iconColor;

  const _InfoChip({
    required this.icon,
    required this.text,
    this.iconColor,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, size: 18, color: iconColor ?? Colors.grey.shade700),
        const SizedBox(width: 8),
        Expanded(
          child: Text(
            text,
            style: TextStyle(color: Colors.grey.shade800),
          ),
        ),
      ],
    );
  }
}
