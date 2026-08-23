import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:provider/provider.dart';
import '../services/reservations_service.dart';
import '../services/payments_service.dart';
import '../services/auth_service.dart';
import '../models/reservation.dart';
import '../models/payment.dart';
import '../utils/api_response.dart';
import '../widgets/reservation_detail_sheet.dart';
import 'payment_screen.dart';

final _dateFormat = DateFormat('dd.MM.yyyy');

class ReservationsScreen extends StatefulWidget {
  const ReservationsScreen({super.key});

  @override
  State<ReservationsScreen> createState() => ReservationsScreenState();
}

class ReservationsScreenState extends State<ReservationsScreen> {
  Future<List<Reservation>>? _paidFuture;
  Future<List<Reservation>>? _unpaidFuture;
  final Set<int> _processingIds = {};
  // Rezervacije iz "unpaid" liste čije je NAJNOVIJE plaćanje trenutno Processing/Pending — nije
  // samo SEPA, i PayPal/druge redirect metode znaju kasnije da se potvrde nego što app stigne
  // pollati (može potrajati par minuta, i u test modu). Bez ovoga bi "Plati ponovo" dugme bilo
  // ponuđeno i dok je plaćanje već u toku, što je zbunjujuće (i backend bi ionako odbio novi
  // checkout dok postoji Processing plaćanje — vidi ValidateAndPrepareCheckoutAsync) — bolje
  // odmah pokazati da je "u obradi", ne "neplaćeno".
  final Map<int, PaymentDetails> _pendingPayments = {};

  Future<void> _loadData(int userId) async {
    final service = ReservationsService();
    final paidFuture = service.fetchPaidReservations(userId);
    final unpaidFuture = service.fetchUnpaidReservations(userId);
    final paid = await paidFuture;
    final unpaid = await unpaidFuture;

    final paymentsService = PaymentsService();
    final pending = <int, PaymentDetails>{};
    await Future.wait(unpaid.map((r) async {
      final payments = await paymentsService.getPaymentsByBooking(r.id);
      final active = payments.where((p) => p.isPendingConfirmation);
      if (active.isNotEmpty) pending[r.id] = active.first;
    }));

    if (!mounted) return;
    setState(() {
      _paidFuture = Future.value(paid);
      _unpaidFuture = Future.value(unpaid);
      _pendingPayments
        ..clear()
        ..addAll(pending);
    });
  }

  /// Ručna provjera statusa jednog "u obradi" plaćanja (npr. korisnik se vratio na ovaj ekran
  /// nakon par minuta) — isti mehanizam kao "Provjeri status" na payment_screen.dart, samo bez
  /// potrebe da se ponovo otvara cijeli payment ekran.
  Future<void> _checkPendingStatus(Reservation r) async {
    final payment = _pendingPayments[r.id];
    if (payment == null) return;
    setState(() => _processingIds.add(r.id));
    try {
      final paymentsService = PaymentsService();
      await paymentsService.confirmPaymentAfterReturn(payment.id, PaymentMethod.stripe);
      if (!mounted) return;
      final completed = await paymentsService.isPaymentCompleted(payment.id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(completed
              ? 'Plaćanje je potvrđeno!'
              : 'Plaćanje je i dalje u obradi. Pokušajte ponovo za koji trenutak.'),
          backgroundColor: completed ? Colors.green : null,
        ),
      );
      final userId = context.read<AuthService>().user?.userId;
      if (userId != null) await _loadData(userId);
    } finally {
      if (mounted) setState(() => _processingIds.remove(r.id));
    }
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

  /// Ručno okidanje ponovnog učitavanja — poziva ga HomeScreen kad korisnik pređe na ovaj tab.
  /// Ekran ostaje mountovan u pozadini (IndexedStack), pa se initState ne poziva ponovo pri
  /// svakom prelasku na tab — bez ovoga korisnik mora ručno povući za refresh da vidi promjene
  /// (npr. otkazivanje rezervacije urađeno na desktop app-u).
  Future<void> refresh() async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId != null) await _loadData(userId);
  }

  void _openDetails(Reservation reservation) {
    showReservationDetailSheet(
      context,
      reservation: reservation,
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
              Tab(text: 'Ostale'),
            ],
          ),
        ),
        body: userId == null
            ? const Center(child: Text('Niste prijavljeni.'))
            : TabBarView(
                children: [
                  _ReservationsList(
                    future: _paidFuture,
                    onRefresh: () => _loadData(userId),
                    onOpenDetails: _openDetails,
                    onCancel: (r) => _confirmAndCancel(r, paid: true),
                    processingIds: _processingIds,
                    emptyMessage: 'Nemate plaćenih rezervacija.',
                    paid: true,
                  ),
                  _ReservationsList(
                    future: _unpaidFuture,
                    onRefresh: () => _loadData(userId),
                    onOpenDetails: _openDetails,
                    onPayAgain: _payAgain,
                    onCancel: (r) => _confirmAndCancel(r, paid: false),
                    processingIds: _processingIds,
                    emptyMessage: 'Nemate ostalih rezervacija.',
                    paid: false,
                    pendingPayments: _pendingPayments,
                    onCheckPending: _checkPendingStatus,
                  ),
                ],
              ),
      ),
    );
  }
}

class _ReservationsList extends StatelessWidget {
  final Future<List<Reservation>>? future;
  final Future<void> Function() onRefresh;
  final void Function(Reservation) onOpenDetails;
  final void Function(Reservation)? onPayAgain;
  final void Function(Reservation)? onCancel;
  final Set<int> processingIds;
  final String emptyMessage;
  final bool paid;
  /// bookingId -> plaćanje koje je trenutno Processing/Pending (npr. SEPA koji čeka potvrdu).
  /// Prazno za "paid" listu (nije relevantno).
  final Map<int, PaymentDetails> pendingPayments;
  final void Function(Reservation)? onCheckPending;

  const _ReservationsList({
    required this.future,
    required this.onRefresh,
    required this.onOpenDetails,
    required this.emptyMessage,
    required this.paid,
    this.onPayAgain,
    this.onCancel,
    this.processingIds = const {},
    this.pendingPayments = const {},
    this.onCheckPending,
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
              final serviceCount = r.services.length;
              final pendingPayment = pendingPayments[r.id];
              final hasPendingPayment = pendingPayment != null;
              // Status 5 = Otkazana, 6 = No-show — plaćanje se ne nudi za njih. Dok postoji
              // plaćanje koje je već Processing/Pending (npr. SEPA čeka potvrdu), ne nudimo
              // "Plati ponovo" — backend bi ionako odbio novi checkout dok on traje, a korisniku
              // je jasnije da vidi "u obradi" nego zbunjujuću grešku.
              final canPayAgain = !paid &&
                  !hasPendingPayment &&
                  onPayAgain != null &&
                  r.status != 5 &&
                  r.status != 6;
              // Otkazivanje ima smisla samo dok rezervacija čeka (1) ili je potvrđena (2) —
              // nakon check-in/check-out/otkazivanja/no-show akcija više nije relevantna.
              final canCancel =
                  onCancel != null && (r.status == 1 || r.status == 2);
              final isProcessing = processingIds.contains(r.id);
              // Status 5/6 (Otkazana/No-show) MORA imati prioritet nad "paid" — inače rezervacija
              // koja je otkazana (ali čije plaćanje iz nekog razloga nije stiglo refundovati) i
              // dalje pogrešno pokazuje zeleno "Plaćeno", umjesto stvarnog statusa.
              final isCancelledOrNoShow = r.status == 5 || r.status == 6;

              return Card(
                color: isCancelledOrNoShow
                    ? Colors.red.shade50
                    : paid
                        ? Colors.green.shade50
                        : hasPendingPayment
                            ? Colors.blue.shade50
                            : Colors.orange.shade50,
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
                              backgroundColor: isCancelledOrNoShow
                                  ? Colors.red.shade100
                                  : paid
                                      ? Colors.green.shade100
                                      : hasPendingPayment
                                          ? Colors.blue.shade100
                                          : Colors.orange.shade100,
                              child: Icon(
                                isCancelledOrNoShow
                                    ? Icons.cancel
                                    : paid
                                        ? Icons.check_circle
                                        : hasPendingPayment
                                            ? Icons.sync
                                            : Icons.hourglass_bottom,
                                color: isCancelledOrNoShow
                                    ? Colors.red.shade800
                                    : paid
                                        ? Colors.green
                                        : hasPendingPayment
                                            ? Colors.blue.shade800
                                            : Colors.orange.shade800,
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
                                    isCancelledOrNoShow
                                        ? r.statusLabel
                                        : paid
                                            ? 'Plaćeno'
                                            : hasPendingPayment
                                                ? 'Plaćanje u obradi'
                                                : r.statusLabel,
                                    style: TextStyle(
                                      color: isCancelledOrNoShow
                                          ? Colors.red.shade700
                                          : paid
                                              ? Colors.green.shade700
                                              : hasPendingPayment
                                                  ? Colors.blue.shade800
                                                  : Colors.orange.shade800,
                                      fontWeight: FontWeight.w600,
                                    ),
                                  ),
                                  if (isCancelledOrNoShow && paid)
                                    Text(
                                      'Rezervacija je otkazana. Ako je bila plaćena, povrat '
                                      'novca je automatski pokrenut.',
                                      style: TextStyle(
                                        fontSize: 12,
                                        color: Colors.red.shade700,
                                      ),
                                    ),
                                  if (hasPendingPayment)
                                    Text(
                                      'Kod nekih načina plaćanja (PayPal, bankovni transfer) '
                                      'potvrda ponekad treba par minuta — potvrdiće se automatski.',
                                      style: TextStyle(
                                        fontSize: 12,
                                        color: Colors.blue.shade700,
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
                        if (hasPendingPayment && onCheckPending != null) ...[
                          const SizedBox(height: 12),
                          SizedBox(
                            width: double.infinity,
                            child: OutlinedButton.icon(
                              icon: isProcessing
                                  ? const SizedBox(
                                      width: 16,
                                      height: 16,
                                      child: CircularProgressIndicator(strokeWidth: 2),
                                    )
                                  : const Icon(Icons.refresh),
                              label: const Text('Provjeri status'),
                              onPressed:
                                  isProcessing ? null : () => onCheckPending!(r),
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
