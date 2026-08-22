import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'providers/auth_provider.dart';
import 'screens/login_screen.dart';
import 'screens/hotels_screen.dart';
import 'screens/bookings_screen.dart';
import 'screens/employees_screen.dart';
import 'screens/users_screen.dart';
import 'screens/rooms_screen.dart';
import 'screens/services_screen.dart';
import 'screens/dashboard_screen.dart';
import 'screens/support_tickets_screen.dart';
import 'screens/room_maintenance_logs_screen.dart';
import 'screens/price_adjustments_screen.dart';
import 'screens/inventory_transactions_screen.dart';
import 'screens/inventory_items_screen.dart';
import 'screens/loyalty_redemptions_screen.dart';
import 'screens/cities_screen.dart';
import 'screens/reports_screen.dart';
import 'utils/role_utils.dart';

void main() {
  runApp(
    MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
      ],
      child: const MyApp(),
    ),
  );
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});
  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Hotel Sistem',
      theme: ThemeData(
        brightness: Brightness.dark,
        primarySwatch: Colors.indigo,
      ),
      home: Consumer<AuthProvider>(
        builder: (context, auth, _) {
          if (auth.isLoading) {
            return const Scaffold(
                body: Center(child: CircularProgressIndicator()));
          }
          return auth.isAuthenticated ? const MainTabs() : const LoginScreen();
        },
      ),
    );
  }
}

class MainTabs extends StatefulWidget {
  const MainTabs({super.key});
  @override
  State<MainTabs> createState() => _MainTabsState();
}

class _MainTabsState extends State<MainTabs>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  late List<Tab> _tabs;
  late List<Widget> _tabViews;

  @override
  void initState() {
    super.initState();
    final auth = Provider.of<AuthProvider>(context, listen: false);
    if (RoleUtils.isAdmin(auth.role)) {
      _tabs = const [
        Tab(text: 'Pregled'),
        Tab(text: 'Uposlenici'),
        Tab(text: 'Korisnici'),
        Tab(text: 'Podrška'),
        Tab(text: 'Cijene'),
        Tab(text: 'Izvještaji'),
      ];
      _tabViews = const [
        DashboardScreen(),
        EmployeesScreen(),
        UsersScreen(),
        SupportTicketsScreen(),
        PriceAdjustmentsScreen(),
        ReportsScreen(),
      ];
    } else if (RoleUtils.isEmployee(auth.role)) {
      _tabs = const [
        Tab(text: 'Hoteli'),
        Tab(text: 'Gradovi'),
        Tab(text: 'Rezervacije'),
        Tab(text: 'Sobe'),
        Tab(text: 'Servisi'),
        Tab(text: 'Podrška'),
        Tab(text: 'Održavanje'),
        Tab(text: 'Cijene'),
        Tab(text: 'Artikli skladišta'),
        Tab(text: 'Skladište'),
        Tab(text: 'Vjernost'),
        Tab(text: 'Izvještaji'),
      ];
      _tabViews = const [
        HotelsScreen(),
        CitiesScreen(),
        BookingsScreen(),
        RoomsScreen(),
        ServicesScreen(),
        SupportTicketsScreen(),
        RoomMaintenanceLogsScreen(),
        PriceAdjustmentsScreen(),
        InventoryItemsScreen(),
        InventoryTransactionsScreen(),
        LoyaltyRedemptionsScreen(),
        ReportsScreen(),
      ];
    } else {
      _tabs = const [Tab(text: 'Greška')];
      _tabViews = const [
        Center(child: Text('Uloga nije podržana ili nemate dozvolu')),
      ];
    }
    _tabController = TabController(length: _tabs.length, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Hotel Sistem'),
        bottom: TabBar(
          controller: _tabController,
          isScrollable: true,
          tabs: _tabs,
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Odjava',
            onPressed: () async {
              await Provider.of<AuthProvider>(context, listen: false).logout();
            },
          ),
        ],
      ),
      body: TabBarView(
        controller: _tabController,
        children: _tabViews,
      ),
    );
  }
}
