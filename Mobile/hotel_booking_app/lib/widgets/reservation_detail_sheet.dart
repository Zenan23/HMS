import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import '../models/reservation.dart';
import '../models/room.dart';
import '../services/reservations_service.dart';
import '../services/rooms_service.dart';

final _dateFormat = DateFormat('dd.MM.yyyy');

String _formatDate(DateTime? value) {
  if (value == null) return '-';
  return _dateFormat.format(value.toLocal());
}

Future<void> showReservationDetailSheet(
  BuildContext context, {
  required Reservation reservation,
}) {
  return showModalBottomSheet<void>(
    context: context,
    isScrollControlled: true,
    shape: const RoundedRectangleBorder(
      borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
    ),
    builder: (ctx) => _ReservationDetailSheet(
      reservation: reservation,
    ),
  );
}

class _ReservationDetailSheet extends StatefulWidget {
  final Reservation reservation;

  const _ReservationDetailSheet({
    required this.reservation,
  });

  @override
  State<_ReservationDetailSheet> createState() =>
      _ReservationDetailSheetState();
}

class _ReservationDetailSheetState extends State<_ReservationDetailSheet> {
  Reservation? _details;
  Room? _room;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadDetails();
  }

  Future<void> _loadDetails() async {
    final fetched =
        await ReservationsService().getReservationById(widget.reservation.id);
    final room = await RoomsService().getRoomById(
      (fetched ?? widget.reservation).roomId,
    );
    if (!mounted) return;
    setState(() {
      _details = fetched ?? widget.reservation;
      _room = room;
      _loading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final r = _details ?? widget.reservation;

    return Padding(
      padding: EdgeInsets.only(
        bottom: MediaQuery.of(context).viewInsets.bottom,
      ),
      child: ConstrainedBox(
        constraints: BoxConstraints(
          maxHeight: MediaQuery.of(context).size.height * 0.85,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const SizedBox(height: 8),
            Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey.shade400,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 12, 8),
              child: Row(
                children: [
                  CircleAvatar(
                    backgroundColor: Colors.green.shade100,
                    child:
                        const Icon(Icons.receipt_long, color: Colors.green),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          'Rezervacija #${r.id}',
                          style: Theme.of(context)
                              .textTheme
                              .titleLarge
                              ?.copyWith(fontWeight: FontWeight.bold),
                        ),
                        Text(
                          'Plaćena rezervacija',
                          style: TextStyle(color: Colors.green.shade700),
                        ),
                      ],
                    ),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.pop(context),
                  ),
                ],
              ),
            ),
            const Divider(height: 1),
            Flexible(
              child: _loading
                  ? const Padding(
                      padding: EdgeInsets.all(32),
                      child: Center(child: CircularProgressIndicator()),
                    )
                  : ListView(
                      shrinkWrap: true,
                      padding: const EdgeInsets.all(20),
                      children: [
                          _DetailRow(
                            icon: Icons.calendar_today,
                            label: 'Check-in',
                            value: _formatDate(r.checkInDate),
                          ),
                          _DetailRow(
                            icon: Icons.event,
                            label: 'Check-out',
                            value: _formatDate(r.checkOutDate),
                          ),
                          _DetailRow(
                            icon: Icons.people_outline,
                            label: 'Broj gostiju',
                            value: '${r.numberOfGuests}',
                          ),
                          _DetailRow(
                            icon: Icons.info_outline,
                            label: 'Status',
                            value: r.statusLabel,
                          ),
                          if (_room != null) ...[
                            const SizedBox(height: 8),
                            Text(
                              'Soba',
                              style: Theme.of(context)
                                  .textTheme
                                  .titleSmall
                                  ?.copyWith(fontWeight: FontWeight.w600),
                            ),
                            const SizedBox(height: 8),
                            _DetailRow(
                              icon: Icons.hotel,
                              label: 'Hotel',
                              value: _room!.hotelName,
                            ),
                            _DetailRow(
                              icon: Icons.meeting_room_outlined,
                              label: 'Soba',
                              value:
                                  '${_room!.roomNumber} (${_room!.roomTypeString})',
                            ),
                          ],
                          const SizedBox(height: 12),
                          Text(
                            'Dodatne usluge',
                            style: Theme.of(context)
                                .textTheme
                                .titleSmall
                                ?.copyWith(fontWeight: FontWeight.w600),
                          ),
                          const SizedBox(height: 8),
                          if (r.services.isEmpty)
                            _DetailRow(
                              icon: Icons.spa_outlined,
                              label: 'Usluge',
                              value: 'Nema dodatnih usluga',
                            )
                          else
                            ...r.services.map(
                              (s) => _ServiceTile(service: s),
                            ),
                          if (r.specialRequests.trim().isNotEmpty) ...[
                            const SizedBox(height: 12),
                            _DetailRow(
                              icon: Icons.notes,
                              label: 'Posebni zahtjevi',
                              value: r.specialRequests,
                            ),
                          ],
                          const SizedBox(height: 16),
                          Container(
                            width: double.infinity,
                            padding: const EdgeInsets.all(16),
                            decoration: BoxDecoration(
                              color: Colors.green.shade50,
                              borderRadius: BorderRadius.circular(12),
                              border: Border.all(color: Colors.green.shade200),
                            ),
                            child: Row(
                              children: [
                                Icon(Icons.payments_outlined,
                                    color: Colors.green.shade700),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: Text(
                                    'Ukupno plaćeno',
                                    style: TextStyle(
                                      fontWeight: FontWeight.w600,
                                      color: Colors.green.shade900,
                                    ),
                                  ),
                                ),
                                Text(
                                  '${r.totalPrice.toStringAsFixed(2)} EUR',
                                  style: TextStyle(
                                    fontSize: 18,
                                    fontWeight: FontWeight.bold,
                                    color: Colors.green.shade900,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
            ),
          ],
        ),
      ),
    );
  }
}

class _DetailRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final String value;
  final Color? valueColor;

  const _DetailRow({
    required this.icon,
    required this.label,
    required this.value,
    this.valueColor,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(icon, size: 20, color: Theme.of(context).colorScheme.primary),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  label,
                  style: TextStyle(
                    fontSize: 12,
                    color: Colors.grey.shade600,
                  ),
                ),
                Text(
                  value,
                  style: TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w500,
                    color: valueColor,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _ServiceTile extends StatelessWidget {
  final ReservationServiceItem service;

  const _ServiceTile({required this.service});

  @override
  Widget build(BuildContext context) {
    final name = service.serviceName?.trim().isNotEmpty == true
        ? service.serviceName!
        : 'Usluga #${service.serviceId}';

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      elevation: 0,
      color: Colors.grey.shade50,
      child: ListTile(
        dense: true,
        leading: const Icon(Icons.room_service_outlined),
        title: Text(name),
        subtitle: Text('Količina: ${service.quantity}'),
        trailing: Text(
          '${service.lineTotal.toStringAsFixed(2)} EUR',
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
      ),
    );
  }
}
