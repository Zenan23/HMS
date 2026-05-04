import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../models/payment.dart';
import '../services/payments_service.dart';
import '../services/auth_service.dart';
import '../screens/home_screen.dart';

class PaymentScreen extends StatefulWidget {
  final int bookingId;
  final double amount;
  final String currency;
  const PaymentScreen({
    super.key,
    required this.bookingId,
    required this.amount,
    this.currency = 'EUR',
  });

  @override
  State<PaymentScreen> createState() => _PaymentScreenState();
}

class _PaymentScreenState extends State<PaymentScreen> {
  final _formKey = GlobalKey<FormState>();
  PaymentMethod _method = PaymentMethod.stripe;
  final _sessionController = TextEditingController();
  String? _description;
  bool _loading = false;
  String? _error;
  String? _success;
  int? _pendingPaymentId;

  @override
  void dispose() {
    _sessionController.dispose();
    super.dispose();
  }

  Future<void> _openCheckoutAndPoll() async {
    if (!(_formKey.currentState?.validate() ?? true)) return;
    _formKey.currentState?.save();

    setState(() {
      _loading = true;
      _error = null;
      _success = null;
    });

    final userId = Provider.of<AuthService>(context, listen: false).user?.userId;
    if (userId == null) {
      setState(() {
        _loading = false;
        _error = 'Niste prijavljeni.';
      });
      return;
    }

    final dto = CreateHostedCheckoutDto(
      userId: userId,
      bookingId: widget.bookingId,
      amount: widget.amount,
      paymentMethod: _method,
      currency: widget.currency,
      description: _description,
    );

    final checkout = await PaymentsService().startHostedCheckout(dto);
    if (checkout == null) {
      setState(() {
        _loading = false;
        _error = 'Neuspješno kreiranje plaćanja. Provjerite API i konfiguraciju.';
      });
      return;
    }

    _pendingPaymentId = checkout.paymentId;
    final uri = Uri.parse(checkout.redirectUrl);
    final launched = await launchUrl(uri, mode: LaunchMode.externalApplication);
    if (!launched) {
      setState(() {
        _loading = false;
        _error = 'Nije moguće otvoriti link plaćanja.';
      });
      return;
    }

    setState(() {
      _loading = false;
      _success =
          'Završite plaćanje u pregledniku. Aplikacija provjerava status… (Stripe: po potrebi unesite session_id ispod i potvrdite.)';
    });

    await _pollUntilComplete(checkout.paymentId);
  }

  Future<void> _pollUntilComplete(int paymentId) async {
    final svc = PaymentsService();
    for (var i = 0; i < 90; i++) {
      await Future.delayed(const Duration(seconds: 2));
      if (!mounted) return;
      final done = await svc.isPaymentCompleted(paymentId);
      if (done) {
        setState(() => _success = 'Plaćanje potvrđeno!');
        await Future.delayed(const Duration(seconds: 1));
        if (mounted) {
          Navigator.of(context).pushAndRemoveUntil(
            MaterialPageRoute(
                builder: (_) => const HomeScreen(initialTabIndex: 1)),
            (route) => false,
          );
        }
        return;
      }
    }
    if (mounted) {
      setState(() {
        _error =
            'Plaćanje još nije potvrđeno (webhook ili sandbox kašnjenje). Pokušajte "Potvrdi Stripe sesiju" ili sačekajte.';
      });
    }
  }

  Future<void> _finalizeStripeManual() async {
    final sid = _sessionController.text.trim();
    if (sid.isEmpty) return;
    setState(() {
      _loading = true;
      _error = null;
    });
    final ok = await PaymentsService().finalizeStripeSession(sid);
    setState(() => _loading = false);
    if (ok && _pendingPaymentId != null) {
      await _pollUntilComplete(_pendingPaymentId!);
    } else {
      setState(() => _error = 'Finalize nije uspio.');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Plaćanje')),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Form(
          key: _formKey,
          child: ListView(
            children: [
              Text(
                'Iznos: ${widget.amount.toStringAsFixed(2)} ${widget.currency}',
                style: const TextStyle(fontSize: 18),
              ),
              const SizedBox(height: 8),
              const Text('Metoda plaćanja', style: TextStyle(fontSize: 12)),
              const SizedBox(height: 8),
              SegmentedButton<PaymentMethod>(
                segments: const [
                  ButtonSegment(
                    value: PaymentMethod.stripe,
                    label: Text('Stripe'),
                  ),
                  ButtonSegment(
                    value: PaymentMethod.paypal,
                    label: Text('PayPal'),
                  ),
                ],
                selected: {_method},
                onSelectionChanged: (Set<PaymentMethod> selection) {
                  if (selection.isNotEmpty) {
                    setState(() => _method = selection.first);
                  }
                },
              ),
              const SizedBox(height: 16),
              TextFormField(
                decoration: const InputDecoration(
                    labelText: 'Opis (opciono)', counterText: ''),
                maxLength: 500,
                onSaved: (v) => _description = v,
              ),
              const SizedBox(height: 8),
              TextFormField(
                controller: _sessionController,
                decoration: const InputDecoration(
                  labelText: 'Stripe session_id (nakon povratka, opciono)',
                  hintText: 'cs_test_...',
                ),
              ),
              const SizedBox(height: 12),
              if (_error != null) ...[
                Text(_error!, style: const TextStyle(color: Colors.red)),
                const SizedBox(height: 12),
              ],
              if (_success != null) ...[
                Text(_success!, style: const TextStyle(color: Colors.green)),
                const SizedBox(height: 12),
              ],
              _loading
                  ? const Center(child: CircularProgressIndicator())
                  : Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        ElevatedButton(
                          onPressed: _openCheckoutAndPoll,
                          child: const Text('Plati (otvori checkout)'),
                        ),
                        const SizedBox(height: 8),
                        OutlinedButton(
                          onPressed: _loading ? null : _finalizeStripeManual,
                          child: const Text('Potvrdi Stripe sesiju (ručno)'),
                        ),
                      ],
                    ),
            ],
          ),
        ),
      ),
    );
  }
}
