import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../services/reservations_service.dart';
import '../services/loyalty_points_redemptions_service.dart';
import '../models/loyalty_points_redemption.dart';
import '../services/auth_service.dart';
import '../models/reservation.dart';
import '../widgets/reservation_detail_sheet.dart';

final _dateFormat = DateFormat('dd.MM.yyyy');

class ReservationsScreen extends StatefulWidget {
  const ReservationsScreen({Key? key}) : super(key: key);

  @override
  State<ReservationsScreen> createState() => _ReservationsScreenState();
}

class _ReservationsScreenState extends State<ReservationsScreen> {
  Future<List<Reservation>>? _future;
  List<LoyaltyPointsRedemption> _loyaltyForBookings = [];

  Future<void> _loadData(int userId) async {
    final reservations =
        await ReservationsService().fetchPaidReservations(userId);
    final loyaltyService = LoyaltyPointsRedemptionsService();
    final allRedemptions = await loyaltyService.getByUserId(userId);
    setState(() {
      _future = Future.value(reservations);
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

  String _formatDate(DateTime? value) {
    if (value == null) return '-';
    return _dateFormat.format(value.toLocal());
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

  @override
  Widget build(BuildContext context) {
    final userId = context.watch<AuthService>().user?.userId;
    return Scaffold(
      appBar: AppBar(title: const Text('Plaćene rezervacije')),
      body: userId == null
          ? const Center(child: Text('Niste prijavljeni.'))
          : (_future == null
              ? const Center(child: CircularProgressIndicator())
              : FutureBuilder<List<Reservation>>(
                  future: _future,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Center(child: CircularProgressIndicator());
                    }
                    if (snapshot.hasError) {
                      return const Center(
                          child: Text('Greška pri dohvatu rezervacija.'));
                    }
                    final reservations = snapshot.data ?? [];
                    if (reservations.isEmpty) {
                      return Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: const [
                            Icon(Icons.info_outline,
                                size: 64, color: Colors.grey),
                            SizedBox(height: 16),
                            Text(
                              'Nemate plaćenih rezervacija.',
                              style:
                                  TextStyle(fontSize: 18, color: Colors.grey),
                            ),
                          ],
                        ),
                      );
                    }
                    return RefreshIndicator(
                      onRefresh: () async {
                        await _loadData(userId);
                      },
                      child: ListView.builder(
                        padding: const EdgeInsets.symmetric(vertical: 8),
                        itemCount: reservations.length,
                        itemBuilder: (context, i) {
                          final r = reservations[i];
                          final loyalty = _loyaltyForBookings
                              .where((l) => l.bookingId == r.id)
                              .toList();
                          final serviceCount = r.services.length;

                          return Card(
                            color: Colors.green.shade50,
                            margin: const EdgeInsets.symmetric(
                                horizontal: 16, vertical: 8),
                            child: InkWell(
                              borderRadius: BorderRadius.circular(12),
                              onTap: () => _openDetails(r),
                              child: Padding(
                                padding: const EdgeInsets.all(16),
                                child: Column(
                                  crossAxisAlignment: CrossAxisAlignment.start,
                                  children: [
                                    Row(
                                      children: [
                                        CircleAvatar(
                                          backgroundColor:
                                              Colors.green.shade100,
                                          child: const Icon(
                                            Icons.check_circle,
                                            color: Colors.green,
                                          ),
                                        ),
                                        const SizedBox(width: 12),
                                        Expanded(
                                          child: Column(
                                            crossAxisAlignment:
                                                CrossAxisAlignment.start,
                                            children: [
                                              Text(
                                                'Rezervacija #${r.id}',
                                                style: const TextStyle(
                                                  fontWeight: FontWeight.bold,
                                                  fontSize: 16,
                                                ),
                                              ),
                                              Text(
                                                'Plaćeno',
                                                style: TextStyle(
                                                  color: Colors.green.shade700,
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
                                      text:
                                          '${r.totalPrice.toStringAsFixed(2)} EUR',
                                    ),
                                    if (serviceCount > 0) ...[
                                      const SizedBox(height: 6),
                                      _InfoChip(
                                        icon: Icons.room_service_outlined,
                                        text:
                                            '$serviceCount dodatne usluge',
                                      ),
                                    ],
                                    if (loyalty.isNotEmpty) ...[
                                      const SizedBox(height: 6),
                                      _InfoChip(
                                        icon: Icons.stars,
                                        text:
                                            'Loyalty: ${loyalty.first.pointsUsed} bodova',
                                        iconColor: Colors.amber.shade800,
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
                )),
    );
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
