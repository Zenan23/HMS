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
  const HomeScreen({Key? key, this.initialTabIndex = 0}) : super(key: key);

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  late int _selectedIndex;

  final List<Widget> _screens = const [
    HotelsScreen(),
    ReservationsScreen(),
    NotificationsScreen(),
    ProfileScreen(),
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
    setState(() {
      _selectedIndex = index;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: _screens[_selectedIndex],
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