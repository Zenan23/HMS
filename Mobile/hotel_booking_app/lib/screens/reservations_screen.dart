import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../services/reservations_service.dart';
import '../services/auth_service.dart';
import '../models/reservation.dart';

class ReservationsScreen extends StatefulWidget {
  const ReservationsScreen({Key? key}) : super(key: key);

  @override
  State<ReservationsScreen> createState() => _ReservationsScreenState();
}

class _ReservationsScreenState extends State<ReservationsScreen> {
  Future<List<Reservation>>? _future;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final auth = context.read<AuthService>();
      final userId = auth.user?.userId;
      if (userId != null) {
        setState(() {
          _future = ReservationsService().fetchPaidReservations(userId);
        });
      }
    });
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
                      return Center(child: Text('Greška pri dohvatu rezervacija.'));
                    }
                    final reservations = snapshot.data ?? [];
                    if (reservations.isEmpty) {
                      return Center(
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: const [
                            Icon(Icons.info_outline, size: 64, color: Colors.grey),
                            SizedBox(height: 16),
                            Text('Nemate plaćenih rezervacija.', style: TextStyle(fontSize: 18, color: Colors.grey)),
                          ],
                        ),
                      );
                    }
                    return RefreshIndicator(
                      onRefresh: () async {
                        setState(() {
                          _future = ReservationsService().fetchPaidReservations(userId);
                        });
                        await _future;
                      },
                      child: ListView.builder(
                        itemCount: reservations.length,
                        itemBuilder: (context, i) {
                          final r = reservations[i];
                          return Card(
                            color: Colors.green[50],
                            margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                            child: ListTile(
                              leading: const Icon(Icons.check_circle, color: Colors.green),
                              title: Text('Rezervacija #${r.id}'),
                              subtitle: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Text('Check-in: ${r.checkInDate != null ? r.checkInDate!.toLocal().toString().split(" ")[0] : ''}'),
                                  Text('Check-out: ${r.checkOutDate != null ? r.checkOutDate!.toLocal().toString().split(" ")[0] : ''}'),
                                  Text('Broj gostiju: ${r.numberOfGuests}'),
                                  Text('Ukupno: ${r.totalPrice} EUR'),
                                ],
                              ),
                              trailing: const Text('Plaćeno', style: TextStyle(color: Colors.green, fontWeight: FontWeight.bold)),
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