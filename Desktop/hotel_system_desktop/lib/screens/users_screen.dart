import 'package:flutter/material.dart';
import 'package:hotel_system_desktop/widgets/user_form.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/pdf_report_service.dart';
import '../utils/date_format_utils.dart';
import '../utils/display_labels.dart';
import 'dart:convert';
import '../widgets/app_dialog_title.dart';
import '../utils/error_helper.dart';

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
  String _searchQuery = '';
  bool _isSearchMode = false;

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
        _isSearchMode = false;
      });
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Greška: ${friendlyErrorMessage(e)}')));
    }
    setState(() => _isLoading = false);
  }

  // Izvještaj mora obuhvatiti cijeli dataset, ne samo trenutno prikazanu
  // stranicu — dohvati sve stranice prije generisanja PDF-a.
  Future<List<Employee>> _fetchAllUsersForExport() async {
    final List<Employee> all = [];
    int page = 1;
    const int size = 100;
    while (true) {
      final response = await ApiService()
          .get('/api/Users?pageNumber=$page&pageSize=$size');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final data = decoded['data'] ?? {};
      final List items = data['items'] ?? [];
      all.addAll(items.map((e) => Employee.fromJson(e)));
      final int totalCount = data['totalCount'] ?? 0;
      if (all.length >= totalCount || items.isEmpty) break;
      page++;
    }
    return all;
  }

  Future<void> _exportUsersPdf() async {
    setState(() => _isLoading = true);
    try {
      final all = await _fetchAllUsersForExport();
      if (mounted) PdfReportService.exportUsers(context, all);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Greška: ${friendlyErrorMessage(e)}')));
      }
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _fetchUserByUsername(String username) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService().get('/api/Users/username/$username');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      
      if (decoded['success'] == true && decoded['data'] != null) {
        final user = Employee.fromJson(decoded['data']);
        setState(() {
          _users = [user];
          _page = 1;
          _totalPages = 1;
          _isSearchMode = true;
        });
      } else {
        setState(() {
          _users = [];
          _page = 1;
          _totalPages = 0;
          _isSearchMode = true;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Korisnik sa tim korisničkim imenom nije pronađen.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      setState(() {
        _users = [];
        _page = 1;
        _totalPages = 0;
        _isSearchMode = true;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Korisnik sa tim korisničkim imenom nije pronađen.'),
          backgroundColor: Colors.orange,
        ),
      );
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
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(
                children: [
                  SizedBox(
                    width: 200,
                    child: TextField(
                      decoration: const InputDecoration(
                        labelText: 'Pretraži po imenu korisnika',
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.search),
                      ),
                      onChanged: (value) {
                        setState(() {
                          _searchQuery = value;
                        });
                      },
                      onSubmitted: (value) {
                        if (value.trim().isEmpty) {
                          _fetchUsers(1);
                        } else {
                          _fetchUserByUsername(value);
                        }
                      },
                    ),
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton(
                    onPressed: () {
                      if (_searchQuery.trim().isEmpty) {
                        _fetchUsers(1);
                      } else {
                        _fetchUserByUsername(_searchQuery);
                      }
                    },
                    child: const Text('Filtriraj'),
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton(
                    onPressed: () {
                      setState(() {
                        _searchQuery = '';
                      });
                      _fetchUsers(1);
                    },
                    child: const Text('Očisti filtere'),
                  ),
                ],
              ),
              Row(
                children: [
                  ElevatedButton.icon(
                    icon: const Icon(Icons.picture_as_pdf),
                    label: const Text('PDF'),
                    onPressed: _exportUsersPdf,
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton.icon(
                    icon: const Icon(Icons.add),
                    label: const Text('Dodaj korisnika'),
                    onPressed: () => _openUserForm(),
                  ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: _users.isEmpty && !_isLoading
                ? Center(
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          _isSearchMode ? Icons.search_off : Icons.people_outline,
                          size: 64,
                          color: Colors.grey,
                        ),
                        const SizedBox(height: 16),
                        Text(
                          _isSearchMode 
                              ? 'Nema rezultata za pretragu'
                              : 'Nema korisnika',
                          style: const TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.grey,
                          ),
                        ),
                        const SizedBox(height: 8),
                        Text(
                          _isSearchMode
                              ? 'Pokušajte sa drugim korisničkim imenom'
                              : 'Dodajte prvog korisnika',
                          style: const TextStyle(
                            color: Colors.grey,
                          ),
                        ),
                      ],
                    ),
                  )
                : SingleChildScrollView(
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
                          DataColumn(label: Text('Akcije')),
                        ],
                        rows: _users
                            .map((emp) => DataRow(cells: [
                                  DataCell(Text(emp.username)),
                                  DataCell(Text(emp.email)),
                                  DataCell(Text(emp.firstName)),
                                  DataCell(Text(emp.lastName)),
                                  DataCell(Text(emp.phoneNumber)),
                                  DataCell(Text(userRoleLabel(emp.role))),
                                  DataCell(Text(emp.isActive ? 'Da' : 'Ne')),
                                  DataCell(Text(formatDisplayDate(emp.lastLoginDate))),
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
                                              title: const AppDialogTitle('Potvrda brisanja'),
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
          if (!_isSearchMode)
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
