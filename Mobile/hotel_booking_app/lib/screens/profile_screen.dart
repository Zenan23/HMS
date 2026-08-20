import 'package:flutter/material.dart';
import 'package:hotel_booking_app/models/user.dart';
import 'package:provider/provider.dart';
import '../services/auth_service.dart';
import '../services/api_service.dart';
import '../services/loyalty_points_redemptions_service.dart';
import '../services/reservations_service.dart';
import '../models/loyalty_points_redemption.dart';
import '../models/reservation.dart';
import '../utils/api_response.dart';
import '../utils/validation_utils.dart';
import 'dart:convert';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});

  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _formKey = GlobalKey<FormState>();
  late TextEditingController _email;
  late TextEditingController _username;
  late TextEditingController _firstName;
  late TextEditingController _lastName;
  late TextEditingController _phoneNumber;
  bool _saving = false;
  String? _status;
  bool _editing = false;
  List<LoyaltyPointsRedemption> _loyaltyHistory = [];
  bool _loadingLoyalty = false;
  int? _loyaltyBalance;
  final _loyaltyService = LoyaltyPointsRedemptionsService();

  @override
  void initState() {
    super.initState();
    final user = context.read<AuthService>().user;
    _email = TextEditingController(text: user?.email ?? '');
    _username = TextEditingController(text: user?.username ?? '');
    _firstName = TextEditingController(text: user?.firstName ?? '');
    _lastName = TextEditingController(text: user?.lastName ?? '');
    _phoneNumber = TextEditingController(text: user?.phoneNumber ?? '');
    _loadLoyaltyHistory();
  }

  Future<void> _loadLoyaltyHistory() async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId == null) return;
    if (mounted) setState(() => _loadingLoyalty = true);
    try {
      final history = await _loyaltyService.getByUserId(userId);
      final balance = await _loyaltyService.getBalance(userId);
      if (mounted) {
        setState(() {
          _loyaltyHistory = history;
          _loyaltyBalance = balance;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _loyaltyHistory = [];
          _loyaltyBalance = null;
        });
      }
    } finally {
      if (mounted) setState(() => _loadingLoyalty = false);
    }
  }

  Future<void> _openRedeemDialog() async {
    final userId = context.read<AuthService>().user?.userId;
    if (userId == null || _loyaltyBalance == null) return;

    if (_loyaltyBalance! <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Nemate dostupnih bodova za iskorištavanje.')),
      );
      return;
    }

    List<Reservation> paidReservations;
    try {
      paidReservations = await ReservationsService().fetchPaidReservations(userId);
    } catch (_) {
      paidReservations = [];
    }

    if (paidReservations.isEmpty) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
              content: Text('Nemate plaćenih rezervacija za koje možete iskoristiti bodove.')),
        );
      }
      return;
    }

    if (!mounted) return;

    int? selectedBookingId = paidReservations.first.id;
    final pointsController = TextEditingController();
    String? dialogError;
    bool submitting = false;

    try {
      await showDialog<void>(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setDialogState) => AlertDialog(
          title: const Text('Iskoristi bodove'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Dostupno: $_loyaltyBalance bodova (100 bodova = 5 EUR)'),
                const SizedBox(height: 12),
                DropdownButtonFormField<int>(
                  value: selectedBookingId,
                  decoration: const InputDecoration(labelText: 'Rezervacija'),
                  items: paidReservations
                      .map((r) => DropdownMenuItem(
                            value: r.id,
                            child: Text('BK-${r.id.toString().padLeft(6, '0')}'),
                          ))
                      .toList(),
                  onChanged: (v) => setDialogState(() => selectedBookingId = v),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: pointsController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(labelText: 'Broj bodova'),
                ),
                if (dialogError != null) ...[
                  const SizedBox(height: 8),
                  Text(dialogError!, style: const TextStyle(color: Colors.red)),
                ],
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: submitting ? null : () => Navigator.pop(ctx),
              child: const Text('Otkaži'),
            ),
            ElevatedButton(
              onPressed: submitting
                  ? null
                  : () async {
                      final points = int.tryParse(pointsController.text.trim());
                      if (points == null || points <= 0) {
                        setDialogState(() => dialogError = 'Unesite ispravan broj bodova.');
                        return;
                      }
                      if (points > _loyaltyBalance!) {
                        setDialogState(() =>
                            dialogError = 'Nemate dovoljno bodova (dostupno: $_loyaltyBalance).');
                        return;
                      }
                      if (selectedBookingId == null) {
                        setDialogState(() => dialogError = 'Odaberite rezervaciju.');
                        return;
                      }
                      setDialogState(() {
                        submitting = true;
                        dialogError = null;
                      });
                      try {
                        await _loyaltyService.redeem(
                          userId: userId,
                          bookingId: selectedBookingId!,
                          pointsUsed: points,
                        );
                        if (ctx.mounted) Navigator.pop(ctx);
                        await _loadLoyaltyHistory();
                        if (mounted) {
                          ScaffoldMessenger.of(context).showSnackBar(
                            const SnackBar(content: Text('Bodovi su uspješno iskorišteni.')),
                          );
                        }
                      } on ApiException catch (e) {
                        setDialogState(() {
                          submitting = false;
                          dialogError = e.message;
                        });
                      } catch (_) {
                        setDialogState(() {
                          submitting = false;
                          dialogError = 'Greška pri iskorištavanju bodova.';
                        });
                      }
                    },
              child: submitting
                  ? const SizedBox(
                      width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2))
                  : const Text('Potvrdi'),
            ),
          ],
        ),
      ),
    );
    } finally {
      pointsController.dispose();
    }
  }

  @override
  void dispose() {
    _email.dispose();
    _username.dispose();
    _firstName.dispose();
    _lastName.dispose();
    _phoneNumber.dispose();
    super.dispose();
  }

  Future<void> _confirmLogout() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Odjava'),
        content: const Text('Da li ste sigurni da se želite odjaviti?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(false),
            child: const Text('Odustani'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () => Navigator.of(ctx).pop(true),
            child: const Text('Odjavi se'),
          ),
        ],
      ),
    );
    if (confirmed != true || !mounted) return;
    final auth = context.read<AuthService>();
    await auth.logout();
    if (!mounted) return;
    Navigator.pushReplacementNamed(context, '/login');
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) return;
    final auth = context.read<AuthService>();
    final user = auth.user!;
    setState(() {
      _saving = true;
      _status = null;
    });
    try {
      final resp = await ApiService.put('/Users/${user.userId}', {
        'id': user.userId,
        'username': _username.text.trim(),
        'email': _email.text.trim(),
        'firstName': _firstName.text.trim(),
        'lastName': _lastName.text.trim(),
        'phoneNumber': _phoneNumber.text.trim(),
        'role': user.role,
        'isActive': true,
      });
      if (resp.statusCode == 200) {
        final data = jsonDecode(resp.body);
        final updated = data['data'] ?? data;
        final updatedUser = User(
          userId: user.userId,
          token: user.token,
          email: updated['email'] ?? _email.text.trim(),
          username: updated['username'] ?? _username.text.trim(),
          firstName: updated['firstName'] ?? _firstName.text.trim(),
          lastName: updated['lastName'] ?? _lastName.text.trim(),
          phoneNumber: updated['phoneNumber'] ?? _phoneNumber.text.trim(),
          role: user.role,
          expiresAt: user.expiresAt,
        );
        auth.updateLocalUser(updatedUser);
        setState(() {
          _status = 'Sačuvano.';
        });
      } else {
        setState(() {
          _status = 'Greška pri čuvanju.';
        });
      }
    } catch (_) {
      setState(() {
        _status = 'Greška pri povezivanju sa serverom.';
      });
    } finally {
      setState(() {
        _saving = false;
      });
    }
  }

@override
Widget build(BuildContext context) {
  final auth = context.watch<AuthService>();
  final user = auth.user;
  if (user == null) {
    return const Scaffold(body: Center(child: Text('Niste prijavljeni.')));
  }

  return Scaffold(
    appBar: AppBar(
      title: const Text('Profil'),
      centerTitle: true,
      elevation: 2,
    ),
    body: SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Card(
        elevation: 4,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _buildTextField(
                  controller: _email,
                  label: 'Email',
                  icon: Icons.email,
                  enabled: _editing,
                  keyboardType: TextInputType.emailAddress,
                  validator: ValidationUtils.validateEmail,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _username,
                  label: 'Korisničko ime',
                  icon: Icons.person,
                  enabled: _editing,
                  validator: ValidationUtils.validateUsername,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _firstName,
                  label: 'Ime',
                  icon: Icons.badge,
                  enabled: _editing,
                  validator: ValidationUtils.validateFirstName,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _lastName,
                  label: 'Prezime',
                  icon: Icons.badge_outlined,
                  enabled: _editing,
                  validator: ValidationUtils.validateLastName,
                ),
                const SizedBox(height: 16),
                _buildTextField(
                  controller: _phoneNumber,
                  label: 'Telefon',
                  icon: Icons.phone,
                  enabled: _editing,
                  keyboardType: TextInputType.phone,
                  validator: ValidationUtils.validatePhoneNumber,
                ),
                const SizedBox(height: 24),
                if (_status != null) ...[
                  Text(
                    _status!,
                    style: TextStyle(
                      color: _status == 'Sačuvano.' ? Colors.green : Colors.red,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 12),
                ],
                Row(
                  children: [
                    Expanded(
                      child: ElevatedButton.icon(
                        onPressed: _editing
                            ? (_saving ? null : _save)
                            : () {
                                setState(() {
                                  _editing = true;
                                });
                              },
                        icon: _editing
                            ? const Icon(Icons.save)
                            : const Icon(Icons.edit),
                        label: _saving
                            ? const CircularProgressIndicator()
                            : Text(_editing ? 'Sačuvaj' : 'Uredi'),
                        style: ElevatedButton.styleFrom(
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(8)),
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    ElevatedButton.icon(
                      onPressed: _confirmLogout,
                      icon: const Icon(Icons.logout),
                      label: const Text('Odjavi se'),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.red,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8)),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 24),
                ElevatedButton.icon(
                  onPressed: () =>
                      Navigator.pushNamed(context, '/support-tickets'),
                  icon: const Icon(Icons.support_agent),
                  label: const Text('Tiketi podrške'),
                ),
                const SizedBox(height: 24),
                Row(
                  children: [
                    const Text('Loyalty bodovi',
                        style: TextStyle(
                            fontSize: 18, fontWeight: FontWeight.bold)),
                    const Spacer(),
                    if (!_loadingLoyalty && _loyaltyBalance != null)
                      Chip(
                        avatar: const Icon(Icons.stars,
                            color: Colors.amber, size: 18),
                        label: Text('$_loyaltyBalance bodova'),
                      ),
                  ],
                ),
                const SizedBox(height: 8),
                if (!_loadingLoyalty)
                  Align(
                    alignment: Alignment.centerLeft,
                    child: OutlinedButton.icon(
                      onPressed: _openRedeemDialog,
                      icon: const Icon(Icons.redeem),
                      label: const Text('Iskoristi bodove'),
                    ),
                  ),
                const SizedBox(height: 16),
                const Text('Historija iskorištenih bodova',
                    style:
                        TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                const SizedBox(height: 8),
                if (_loadingLoyalty)
                  const Center(child: CircularProgressIndicator())
                else if (_loyaltyHistory.isEmpty)
                  const Text('Nema iskorištenih loyalty bodova.')
                else
                  ..._loyaltyHistory.map((r) => ListTile(
                        leading: const Icon(Icons.stars, color: Colors.amber),
                        title: Text('Rezervacija #${r.bookingId}'),
                        subtitle: Text(
                            '${r.pointsUsed} bodova • ${r.equivalentValueAmount.toStringAsFixed(2)} EUR'),
                      )),
              ],
            ),
          ),
        ),
      ),
    ),
  );
}

Widget _buildTextField({
  required TextEditingController controller,
  required String label,
  required IconData icon,
  bool enabled = true,
  TextInputType? keyboardType,
  String? Function(String?)? validator,
}) {
  return TextFormField(
    controller: controller,
    decoration: InputDecoration(
      labelText: label,
      prefixIcon: Icon(icon),
      filled: true,
      fillColor: Colors.grey.shade100,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
      ),
    ),
    keyboardType: keyboardType,
    validator: validator,
    enabled: enabled,
  );
}

}