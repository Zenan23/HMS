import 'dart:convert';
import 'package:flutter/material.dart';
import '../models/loyalty_points_redemption.dart';
import '../models/user.dart';
import '../services/api_service.dart';
import '../services/loyalty_points_redemption_service.dart';
import '../services/pdf_report_service.dart';
import '../utils/date_format_utils.dart';
import '../utils/error_helper.dart';
import '../widgets/loyalty_redemption_form.dart';

class LoyaltyRedemptionsScreen extends StatefulWidget {
  const LoyaltyRedemptionsScreen({super.key});

  @override
  State<LoyaltyRedemptionsScreen> createState() =>
      _LoyaltyRedemptionsScreenState();
}

class _LoyaltyRedemptionsScreenState extends State<LoyaltyRedemptionsScreen> {
  final _service = LoyaltyPointsRedemptionService();
  int _page = 1;
  final int _pageSize = 10;
  bool _isLoading = false;
  List<LoyaltyPointsRedemption> _redemptions = [];
  int? _selectedUserId;
  final _bookingIdController = TextEditingController();
  List<Employee> _users = [];
  int? _selectedUserBalance;

  @override
  void initState() {
    super.initState();
    _fetchUsers();
    _fetchRedemptions();
  }

  @override
  void dispose() {
    _bookingIdController.dispose();
    super.dispose();
  }

  Future<void> _fetchUsers() async {
    try {
      final resp = await ApiService().get('/api/Users/role/0');
      final decoded = jsonDecode(resp.body);
      final List items = (decoded['data'] ?? []) as List;
      _users = items.map((e) => Employee.fromJson(e)).toList().cast<Employee>();
    } catch (_) {}
    if (mounted) setState(() {});
  }

  Future<void> _fetchRedemptions() async {
    setState(() => _isLoading = true);
    try {
      final bookingId = int.tryParse(_bookingIdController.text);
      if (_selectedUserId != null) {
        _redemptions = await _service.getByUserId(_selectedUserId!);
        // Balans daje kontekst zaposleniku prije nego doda ručnu korekciju
        // (npr. da vidi da korisnik nema dovoljno bodova za traženo iskorištenje).
        _selectedUserBalance = await _service.getBalance(_selectedUserId!);
      } else if (bookingId != null) {
        _redemptions = await _service.getByBookingId(bookingId);
        _selectedUserBalance = null;
      } else {
        final result =
            await _service.getPaged(pageNumber: _page, pageSize: _pageSize);
        _redemptions = result.items;
        _selectedUserBalance = null;
      }
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  // Izvještaj mora obuhvatiti cijeli dataset, ne samo trenutno prikazanu
  // stranicu — ako je aktivan filter po korisniku/rezervaciji, taj poziv
  // već vraća sve rezultate; u suprotnom dohvati sve stranice.
  Future<List<LoyaltyPointsRedemption>> _fetchAllRedemptionsForExport() async {
    final bookingId = int.tryParse(_bookingIdController.text);
    if (_selectedUserId != null) {
      return _service.getByUserId(_selectedUserId!);
    }
    if (bookingId != null) {
      return _service.getByBookingId(bookingId);
    }
    final List<LoyaltyPointsRedemption> all = [];
    int page = 1;
    const int size = 100;
    while (true) {
      final result = await _service.getPaged(pageNumber: page, pageSize: size);
      all.addAll(result.items);
      if (all.length >= result.totalCount || result.items.isEmpty) break;
      page++;
    }
    return all;
  }

  Future<void> _exportRedemptionsPdf() async {
    setState(() => _isLoading = true);
    try {
      final all = await _fetchAllRedemptionsForExport();
      if (mounted) PdfReportService.exportLoyaltyRedemptions(context, all);
    } catch (e) {
      if (mounted) showApiError(context, e);
    }
    if (mounted) setState(() => _isLoading = false);
  }

  Future<void> _openForm({LoyaltyPointsRedemption? redemption}) async {
    final result = await showDialog<bool>(
      context: context,
      builder: (_) => LoyaltyRedemptionFormDialog(redemption: redemption),
    );
    if (result == true) _fetchRedemptions();
  }

  String _userLabel(Employee u) {
    final name = u.fullName.isNotEmpty ? u.fullName : u.username;
    return name;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Iskorištenja bodova vjernosti'),
        actions: [
          IconButton(
            icon: const Icon(Icons.picture_as_pdf),
            tooltip: 'PDF izvještaj',
            onPressed: _exportRedemptionsPdf,
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _openForm(),
        tooltip: 'Novo iskorištenje',
        child: const Icon(Icons.add),
      ),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            Row(
              children: [
                SizedBox(
                  width: 220,
                  child: DropdownButtonFormField<int?>(
                    value: _selectedUserId,
                    decoration:
                        const InputDecoration(labelText: 'Korisnik'),
                    items: [
                      const DropdownMenuItem<int?>(
                        value: null,
                        child: Text('Svi korisnici'),
                      ),
                      ..._users.map((u) => DropdownMenuItem<int?>(
                            value: u.id,
                            child: Text(_userLabel(u)),
                          )),
                    ],
                    onChanged: (v) => setState(() => _selectedUserId = v),
                  ),
                ),
                const SizedBox(width: 8),
                SizedBox(
                  width: 140,
                  child: TextField(
                    controller: _bookingIdController,
                    decoration:
                        const InputDecoration(labelText: 'ID rezervacije'),
                    keyboardType: TextInputType.number,
                    onSubmitted: (_) => _fetchRedemptions(),
                  ),
                ),
                const SizedBox(width: 8),
                ElevatedButton(
                  onPressed: _fetchRedemptions,
                  child: const Text('Filtriraj'),
                ),
              ],
            ),
            if (_selectedUserId != null && _selectedUserBalance != null)
              Padding(
                padding: const EdgeInsets.only(top: 8),
                child: Align(
                  alignment: Alignment.centerLeft,
                  child: Chip(
                    label: Text('Trenutni balans: $_selectedUserBalance bodova'),
                  ),
                ),
              ),
            const SizedBox(height: 8),
            Expanded(
              child: _isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : _redemptions.isEmpty
                      ? const Center(child: Text('Nema iskorištenja.'))
                      : ListView.builder(
                          itemCount: _redemptions.length,
                          itemBuilder: (context, i) {
                            final r = _redemptions[i];
                            final user = r.userName.isNotEmpty
                                ? r.userName
                                : 'Korisnik #${r.userId}';
                            return Card(
                              child: ListTile(
                                title: Text(
                                    '$user – ${r.pointsUsed} bodova'),
                                subtitle: Text(
                                    '${r.bookingDisplayLabel} • ${r.equivalentValueAmount.toStringAsFixed(2)} EUR\n'
                                    '${formatDisplayDate(r.redeemedAt)}'),
                                isThreeLine: true,
                                trailing: IconButton(
                                  icon: const Icon(Icons.edit),
                                  tooltip: 'Uredi',
                                  onPressed: () =>
                                      _openForm(redemption: r),
                                ),
                              ),
                            );
                          },
                        ),
            ),
          ],
        ),
      ),
    );
  }
}
