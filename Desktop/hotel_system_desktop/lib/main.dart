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
import 'models/user.dart';

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
        brightness: Brightness.light,
        primarySwatch: Colors.indigo,
      ),
      home: Consumer<AuthProvider>(
        builder: (context, auth, _) {
          if (auth.isLoading)
            return const Center(child: CircularProgressIndicator());
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
    if (auth.role == null || auth.role == UserRole.Admin.index) {
      _tabs = const [
        Tab(text: 'Hoteli'),
        Tab(text: 'Uposlenici'),
        Tab(text: 'Korisnici'),
      ];
      _tabViews = const [
        HotelsScreen(),
        EmployeesScreen(),
        UsersScreen(),
      ];
    } else if (auth.role == UserRole.Employee.index) {
      _tabs = const [
        Tab(text: 'Rezervacije'),
        Tab(text: 'Sobe'),
        Tab(text: 'Servisi'),
      ];
      _tabViews = const [
        BookingsScreen(),
        RoomsScreen(),
        ServicesScreen(),
      ];
    }
    else {
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
          tabs: _tabs,
        ),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Logout',
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
