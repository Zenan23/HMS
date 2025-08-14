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
      });
    } catch (e) {
      ScaffoldMessenger.of(context)
          .showSnackBar(SnackBar(content: Text('Greška: $e')));
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
            mainAxisAlignment: MainAxisAlignment.end,
            children: [
              ElevatedButton.icon(
                icon: const Icon(Icons.add),
                label: const Text('Dodaj uposlenika'),
                onPressed: () => _openEmployeeForm(),
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
