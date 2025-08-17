import 'package:flutter/material.dart';
import '../models/user.dart';
import '../widgets/employee_form.dart';
import '../services/api_service.dart';
import 'dart:convert';

class EmployeesScreen extends StatefulWidget {
  const EmployeesScreen({super.key});
  @override
  State<EmployeesScreen> createState() => _EmployeesScreenState();
}

class _EmployeesScreenState extends State<EmployeesScreen> {
  List<Employee> _employees = [];
  bool _isLoading = false;
  String _searchQuery = '';
  bool _isSearchMode = false;

  @override
  void initState() {
    super.initState();
    _fetchEmployees();
  }

  Future<void> _fetchEmployees() async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService().get('/api/Users/role/1');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      final List data = decoded['data'] ?? [];
      final employees = data.map((e) => Employee.fromJson(e)).toList();
      setState(() {
        _employees = employees;
        _isSearchMode = false;
      });
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Greška: $e')));
    }
    setState(() => _isLoading = false);
  }

  Future<void> _fetchEmployeeByUsername(String username) async {
    setState(() => _isLoading = true);
    try {
      final response = await ApiService().get('/api/Users/employee/username/$username');
      final Map<String, dynamic> decoded = jsonDecode(response.body);
      
      if (decoded['success'] == true && decoded['data'] != null) {
        final employee = Employee.fromJson(decoded['data']);
        // Provjeri da li je korisnik zapravo uposlenik (role = Employee)
        if (employee.role.name == 'Employee') {
          setState(() {
            _employees = [employee];
            _isSearchMode = true;
          });
        } else {
          setState(() {
            _employees = [];
            _isSearchMode = true;
          });
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(
              content: Text('Korisnik sa tim korisničkim imenom nije uposlenik.'),
              backgroundColor: Colors.orange,
            ),
          );
        }
      } else {
        setState(() {
          _employees = [];
          _isSearchMode = true;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Uposlenik sa tim korisničkim imenom nije pronađen.'),
            backgroundColor: Colors.orange,
          ),
        );
      }
    } catch (e) {
      setState(() {
        _employees = [];
        _isSearchMode = true;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Uposlenik sa tim korisničkim imenom nije pronađen.'),
          backgroundColor: Colors.orange,
        ),
      );
    }
    setState(() => _isLoading = false);
  }

  void _openEmployeeForm({Employee? employee}) async {
    final result = await showDialog(
      context: context,
      builder: (context) =>
          EmployeeFormDialog(employee: employee, forceRoleEmployee: true),
    );
    if (result == true) {
      _fetchEmployees();
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
                        labelText: 'Pretraži po imenu uposlenika',
                        border: OutlineInputBorder(),
                        prefixIcon: Icon(Icons.search),
                      ),
                      onChanged: (value) {
                        setState(() {
                          _searchQuery = value;
                        });
                      },
                      onSubmitted: (value) {
                        if (value.isNotEmpty) {
                          _fetchEmployeeByUsername(value);
                        }
                      },
                    ),
                  ),
                  const SizedBox(width: 8),
                  ElevatedButton(
                    onPressed: () {
                      if (_searchQuery.isNotEmpty) {
                        _fetchEmployeeByUsername(_searchQuery);
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
                      _fetchEmployees();
                    },
                    child: const Text('Očisti filtere'),
                  ),
                ],
              ),
              ElevatedButton.icon(
                icon: const Icon(Icons.add),
                label: const Text('Dodaj uposlenika'),
                onPressed: () => _openEmployeeForm(),
              ),
            ],
          ),
          const SizedBox(height: 16),
          Expanded(
            child: _employees.isEmpty && !_isLoading
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
                              : 'Nema uposlenika',
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
                              : 'Dodajte prvog uposlenika',
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
                          DataColumn(label: Text('Kreiran')),
                          DataColumn(label: Text('Ažuriran')),
                          DataColumn(label: Text('Akcije')),
                        ],
                        rows: _employees
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
                                        onPressed: () =>
                                            _openEmployeeForm(employee: emp),
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
                                                  'Da li ste sigurni da želite obrisati uposlenika?'),
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
                                            _fetchEmployees();
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
