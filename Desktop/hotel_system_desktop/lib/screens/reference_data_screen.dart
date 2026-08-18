import 'package:flutter/material.dart';
import '../models/booking.dart';
import '../models/room.dart';
import '../models/support_ticket.dart';
import '../models/user.dart';
import '../utils/display_labels.dart';

/// Pregled preostalih referentnih/šifarnik podataka (Tip sobe, Status
/// rezervacije, Uloga korisnika, Status/Prioritet tiketa podrške).
///
/// Za razliku od Grad/Država (koji su prave DB tabele sa CRUD-om), ove
/// vrijednosti su na backendu definisane kao C# enum (Contracts/Enums/*.cs),
/// ne kao redovi u bazi — vidi TODO-uskladjenost-backend.md /
/// TODO-uskladjenost-db.md. Dodavanje/brisanje vrijednosti bi zahtijevalo
/// izmjenu i rekompajliranje backend koda (novi enum član), pa prava CRUD
/// forma ovdje ne bi bila iskrena prema korisniku — zato je ovo namjerno
/// pregled bez izmjene, sa jasnom napomenom zašto.
class ReferenceDataScreen extends StatefulWidget {
  const ReferenceDataScreen({super.key});

  @override
  State<ReferenceDataScreen> createState() => _ReferenceDataScreenState();
}

class _ReferenceDataScreenState extends State<ReferenceDataScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 5, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  Widget _buildList(List<String> labels) {
    return Column(
      children: [
        Container(
          width: double.infinity,
          color: Colors.amber.withOpacity(0.12),
          padding: const EdgeInsets.all(12),
          child: const Text(
            'Ovo su fiksne sistemske vrijednosti definisane u kodu backend '
            'aplikacije (enum), ne redovi u bazi podataka — zato se ovdje mogu '
            'samo pregledati, ne i dodavati/brisati. Za pretvaranje u pravu '
            'referentnu tabelu sa CRUD-om potrebna je izmjena backend/DB sloja.',
            style: TextStyle(fontSize: 12),
          ),
        ),
        Expanded(
          child: ListView.builder(
            itemCount: labels.length,
            itemBuilder: (context, i) => Card(
              margin: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
              child: ListTile(
                leading: CircleAvatar(child: Text('${i + 1}')),
                title: Text(labels[i]),
              ),
            ),
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Šifarnici (sistemski)'),
        bottom: TabBar(
          controller: _tabController,
          isScrollable: true,
          tabs: const [
            Tab(text: 'Tipovi soba'),
            Tab(text: 'Statusi rezervacije'),
            Tab(text: 'Uloge korisnika'),
            Tab(text: 'Statusi tiketa'),
            Tab(text: 'Prioriteti tiketa'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _buildList(RoomType.values.map(roomTypeLabel).toList()),
          _buildList(BookingStatus.values.map(bookingStatusLabel).toList()),
          _buildList(UserRole.values.map(userRoleLabel).toList()),
          _buildList(SupportTicketStatus.values
              .map(supportTicketStatusLabel)
              .toList()),
          _buildList(SupportTicketPriority.values
              .map(supportTicketPriorityLabel)
              .toList()),
        ],
      ),
    );
  }
}
