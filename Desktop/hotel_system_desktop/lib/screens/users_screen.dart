import 'package:flutter/material.dart';
import 'package:hotel_system_desktop/widgets/user_form.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import 'dart:convert';

class UsersScreen extends StatefulWidget {
  const UsersScreen({super.key});
  @override
  State<UsersScreen> createState() => _UsersScreenState();
}

class _UsersScreenState extends State<UsersScreen> {
  List<Employee> _users = [];
  bool _isLoading = false;
  int _page = 1;
  int _pageSize = 10;
  int _totalPages = 1;

  @override
  void initState() {
    super.initState();
    _fetchUsers(_page);
  }

  Future<void> _fetchUsers(int page) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService()
          .get('/api/Users?pageNumber=$page&pageSize=$_pageSize');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      final users = items.map((e) => Employee.fromJson(e)).toList();
      setState(() {
        _users = users;
        _page = page;
        int totalCount = data['totalCount'] ?? 0;
        _totalPages = (totalCount / _pageSize).ceil();
      });
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Greška: $e')));
    }
    setState(() => _isLoading = false);
  }

  void _openUserForm({Employee? user}) async {
    final result = await showDialog(
      context: context,
      builder: (context) => UserFormDialog(user: user),
    );
    if (result == true) {
      _fetchUsers(_page);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              ElevatedButton.icon(
                icon: const Icon(Icons.add),
                label: const Text('Dodaj korisnika'),
                onPressed: () => _openUserForm(),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: SingleChildScrollView(
    scrollDirection: Axis.vertical,
    child: SingleChildScrollView(
      scrollDirection: Axis.horizontal,
              child: DataTable(
                columns: const [
                  DataColumn(label: Text('Korisničko ime')),
                  DataColumn(label: Text('Email')),
                  DataColumn(label: Text('Ime')),
                  DataColumn(label: Text('Prezime')),
                  DataColumn(label: Text('Telefon')),
                  DataColumn(label: Text('Uloga')),
                  DataColumn(label: Text('Aktivan')),
                  DataColumn(label: Text('Zadnja prijava')),
                  DataColumn(label: Text('Kreiran')),
                  DataColumn(label: Text('Ažuriran')),
                  DataColumn(label: Text('Akcije')),
                ],
                rows: _users
                    .map((emp) => DataRow(cells: [
                          DataCell(Text(emp.username)),
                          DataCell(Text(emp.email)),
                          DataCell(Text(emp.firstName)),
                          DataCell(Text(emp.lastName)),
                          DataCell(Text(emp.phoneNumber)),
                          DataCell(Text(emp.role.name)),
                          DataCell(Text(emp.isActive ? 'Da' : 'Ne')),
                          DataCell(Text(emp.lastLoginDate?.toString() ?? '-')),
                          DataCell(Text(emp.createdAt.toString())),
                          DataCell(Text(emp.updatedAt.toString())),
                          DataCell(Row(
                            children: [
                              IconButton(
                                icon: const Icon(Icons.edit),
                                tooltip: 'Uredi',
                                onPressed: () => _openUserForm(user: emp),
                              ),
                              IconButton(
                                icon: const Icon(Icons.delete),
                                tooltip: 'Obriši',
                                onPressed: () async {
                                  final confirm = await showDialog<bool>(
                                    context: context,
                                    builder: (context) => AlertDialog(
                                      title: const Text('Potvrda brisanja'),
                                      content: const Text(
                                          'Da li ste sigurni da želite obrisati korisnika?'),
                                      actions: [
                                        TextButton(
                                          onPressed: () =>
                                              Navigator.pop(context, false),
                                          child: const Text('Ne'),
                                        ),
                                        TextButton(
                                          onPressed: () =>
                                              Navigator.pop(context, true),
                                          child: const Text('Da'),
                                        ),
                                      ],
                                    ),
                                  );
                                  if (confirm == true) {
                                    await ApiService()
                                        .delete('/api/Users/${emp.id}');
                                    _fetchUsers(_page);
                                  }
                                },
                              ),
                            ],
                          )),
                        ]))
                    .toList(),
              ),
            ),
            ),
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              ElevatedButton(
                onPressed: _page > 1 && !_isLoading
                    ? () => _fetchUsers(_page - 1)
                    : null,
                child: const Text('Prethodna'),
              ),
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16.0),
                child: Text('Stranica $_page / $_totalPages'),
              ),
              ElevatedButton(
                onPressed: _page < _totalPages && !_isLoading
                    ? () => _fetchUsers(_page + 1)
                    : null,
                child: const Text('Sljedeća'),
              ),
            ],
          ),
          if (_isLoading)
            const Padding(
              padding: EdgeInsets.all(16.0),
              child: Center(child: CircularProgressIndicator()),
            ),
        ],
      ),
    );
  }
}
