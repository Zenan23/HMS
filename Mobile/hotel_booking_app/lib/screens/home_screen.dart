import 'package:flutter/material.dart';
import 'hotels_screen.dart';
import 'reservations_screen.dart';
import 'notifications_screen.dart';
import 'profile_screen.dart';
import 'package:provider/provider.dart';
import '../services/notifications_service.dart';
import '../services/auth_service.dart';

class HomeScreen extends StatefulWidget {
  final int initialTabIndex;
  const HomeScreen({super.key, this.initialTabIndex = 0});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late int _selectedIndex;

  // GlobalKey-evi omogućavaju pozivanje refresh() na tabu koji je već mountovan u pozadini
  // (IndexedStack) — initState se poziva samo jednom, pri prvom mountovanju, pa bez ovoga
  // korisnik mora ručno povući za refresh da vidi promjene nastale dok je bio na drugom tabu.
  final _reservationsKey = GlobalKey<ReservationsScreenState>();
  final _notificationsKey = GlobalKey<NotificationsScreenState>();

  late final List<Widget> _screens = [
    const HotelsScreen(),
    ReservationsScreen(key: _reservationsKey),
    NotificationsScreen(key: _notificationsKey),
    const ProfileScreen(),
  ];

  @override
  void initState() {
    super.initState();
    _selectedIndex = widget.initialTabIndex;
    WidgetsBinding.instance.addPostFrameCallback((_) async {
      final auth = context.read<AuthService>();
      if (auth.user != null) {
        await context.read<NotificationsService>().init(context);
      }
    });
  }

  void _onItemTapped(int index) {
    final changed = index != _selectedIndex;
    setState(() {
      _selectedIndex = index;
    });
    if (!changed) return;
    if (index == 1) {
      _reservationsKey.currentState?.refresh();
    } else if (index == 2) {
      _notificationsKey.currentState?.refresh();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // IndexedStack umjesto direktnog indeksiranja liste — tabovi ostaju
      // mountovani u pozadini pa se ne uništavaju usred async operacije
      // (npr. dijalog za iskorištavanje loyalty bodova na Profil tabu), što je
      // uzrokovalo pad aplikacije ("_dependents.isEmpty") ako bi korisnik
      // promijenio tab dok je async poziv u toku.
      body: IndexedStack(
        index: _selectedIndex,
        children: _screens,
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _selectedIndex,
        onTap: _onItemTapped,
        items: [
          const BottomNavigationBarItem(icon: Icon(Icons.hotel), label: 'Hoteli'),
          const BottomNavigationBarItem(icon: Icon(Icons.book_online), label: 'Rezervacije'),
          BottomNavigationBarItem(
            icon: Consumer<NotificationsService>(
              builder: (_, svc, __) {
                final count = svc.unreadCount;
                if (count <= 0) return const Icon(Icons.notifications);
                return Stack(
                  clipBehavior: Clip.none,
                  children: [
                    const Icon(Icons.notifications),
                    Positioned(
                      right: -6,
                      top: -2,
                      child: Container(
                        padding: const EdgeInsets.all(2),
                        decoration: const BoxDecoration(color: Colors.red, shape: BoxShape.circle),
                        constraints: const BoxConstraints(minWidth: 16, minHeight: 16),
                        child: Center(
                          child: Text(
                            count > 9 ? '9+' : '$count',
                            style: const TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ),
                    )
                  ],
                );
              },
            ),
            label: 'Notifikacije',
          ),
          const BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profil'),
        ],
      ),
    );
  }
}