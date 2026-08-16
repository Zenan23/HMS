import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../services/reservations_service.dart';
import '../services/loyalty_points_redemptions_service.dart';
import '../models/loyalty_points_redemption.dart';
import '../services/auth_service.dart';
import '../models/reservation.dart';
import '../widgets/reservation_detail_sheet.dart';
import 'payment_screen.dart';

final _dateFormat = DateFormat('dd.MM.yyyy');

class ReservationsScreen extends StatefulWidget {
  const ReservationsScreen({Key? key}) : super(key: key);

  @override
  State<ReservationsScreen> createState() => _ReservationsScreenState();
}

class _ReservationsScreenState extends State<ReservationsScreen> {
  Future<List<Reservation>>? _paidFuture;
  Future<List<Reservation>>? _unpaidFuture;
  List<LoyaltyPointsRedemption> _loyaltyForBookings = [];

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
                    emptyMessage: 'Nemate plaćenih rezervacija.',
                    paid: true,
                  ),
                  _ReservationsList(
                    future: _unpaidFuture,
                    loyaltyForBookings: _loyaltyForBookings,
                    onRefresh: () => _loadData(userId),
                    onOpenDetails: _openDetails,
                    onPayAgain: _payAgain,
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
                              onPressed: () => onPayAgain!(r),
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
