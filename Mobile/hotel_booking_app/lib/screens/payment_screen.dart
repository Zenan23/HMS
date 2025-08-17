import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../models/payment.dart';
import '../services/payments_service.dart';
import '../services/auth_service.dart';
import '../screens/home_screen.dart';
import '../utils/validation_utils.dart';

class PaymentScreen extends StatefulWidget {
  final int bookingId;
  final double amount;
  final String currency;
  const PaymentScreen({Key? key, required this.bookingId, required this.amount, this.currency = 'USD'}) : super(key: key);

  @override
  State<PaymentScreen> createState() => _PaymentScreenState();
}

class _PaymentScreenState extends State<PaymentScreen> {
  PaymentMethod _method = PaymentMethod.card;
  final _formKey = GlobalKey<FormState>();
  // Card
  String _cardNumber = '';
  int _expiryMonth = 1;
  int _expiryYear = 2024;
  String _cvv = '';
  String _cardholderName = '';
  // PayPal
  String _paypalEmail = '';
  // Bank
  String _bankAccountNumber = '';
  String _bankRoutingNumber = '';
  String _accountHolderName = '';
  String? _bankName;
  // Ostalo
  String? _description;
  bool _loading = false;
  String? _error;
  String? _success;

  void _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; _success = null; });
    _formKey.currentState!.save();
    final userId = Provider.of<AuthService>(context, listen: false).user?.userId;
    if (userId == null) {
      setState(() { _loading = false; _error = 'Niste prijavljeni.'; });
      return;
    }
    CreatePaymentDto dto = CreatePaymentDto(
      userId: userId,
      bookingId: widget.bookingId,
      amount: widget.amount,
      paymentMethod: _method,
      currency: widget.currency,
      description: _description,
      cardData: _method == PaymentMethod.card ? CardPaymentData(
        cardNumber: _cardNumber,
        expiryMonth: _expiryMonth,
        expiryYear: _expiryYear,
        cvv: _cvv,
        cardholderName: _cardholderName,
      ) : null,
      payPalData: _method == PaymentMethod.paypal ? PayPalPaymentData(payPalEmail: _paypalEmail) : null,
      bankTransferData: _method == PaymentMethod.bankTransfer ? BankTransferPaymentData(
        bankAccountNumber: _bankAccountNumber,
        bankRoutingNumber: _bankRoutingNumber,
        accountHolderName: _accountHolderName,
        bankName: _bankName,
      ) : null,
    );
    final ok = await PaymentsService().processPayment(dto);
    setState(() { _loading = false; _success = ok ? 'Plaćanje uspješno!' : 'Plaćanje nije uspjelo.'; });
    if (ok) {
      await Future.delayed(const Duration(seconds: 1));
      if (mounted) {
        Navigator.of(context).pushAndRemoveUntil(
          MaterialPageRoute(builder: (_) => const HomeScreen(initialTabIndex: 1)),
          (route) => false,
        );
      }
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
              Text('Iznos: ${widget.amount.toStringAsFixed(2)} ${widget.currency}', style: const TextStyle(fontSize: 18)),
              const SizedBox(height: 16),
              DropdownButtonFormField<PaymentMethod>(
                value: _method,
                items: const [
                  DropdownMenuItem(value: PaymentMethod.card, child: Text('Kartica')),
                  DropdownMenuItem(value: PaymentMethod.paypal, child: Text('PayPal')),
                  DropdownMenuItem(value: PaymentMethod.bankTransfer, child: Text('Bankovni transfer')),
                ],
                onChanged: (v) => setState(() => _method = v!),
                decoration: const InputDecoration(labelText: 'Metoda plaćanja'),
              ),
              const SizedBox(height: 16),
              if (_method == PaymentMethod.card) ...[
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Broj kartice'),
                  maxLength: 19,
                  keyboardType: TextInputType.number,
                  validator: ValidationUtils.validateCardNumber,
                  onSaved: (v) => _cardNumber = v ?? '',
                ),
                Row(
                  children: [
                    Expanded(
                      child: TextFormField(
                        decoration: const InputDecoration(labelText: 'MM'),
                        maxLength: 2,
                        keyboardType: TextInputType.number,
                        validator: (v) => v == null || int.tryParse(v) == null || int.parse(v) < 1 || int.parse(v) > 12 ? 'MM' : null,
                        onSaved: (v) => _expiryMonth = int.tryParse(v ?? '') ?? 1,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: TextFormField(
                        decoration: const InputDecoration(labelText: 'YYYY'),
                        maxLength: 4,
                        keyboardType: TextInputType.number,
                        validator: (v) => v == null || int.tryParse(v) == null || int.parse(v) < 2024 ? 'YYYY' : null,
                        onSaved: (v) => _expiryYear = int.tryParse(v ?? '') ?? 2024,
                      ),
                    ),
                  ],
                ),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'CVV'),
                  maxLength: 4,
                  keyboardType: TextInputType.number,
                  validator: ValidationUtils.validateCVV,
                  onSaved: (v) => _cvv = v ?? '',
                ),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Ime i prezime na kartici'),
                  maxLength: 100,
                  validator: ValidationUtils.validateCardholderName,
                  onSaved: (v) => _cardholderName = v ?? '',
                ),
              ],
              if (_method == PaymentMethod.paypal) ...[
                TextFormField(
                  decoration: const InputDecoration(labelText: 'PayPal email'),
                  keyboardType: TextInputType.emailAddress,
                  validator: ValidationUtils.validateEmail,
                  onSaved: (v) => _paypalEmail = v ?? '',
                ),
              ],
              if (_method == PaymentMethod.bankTransfer) ...[
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Broj bankovnog računa'),
                  validator: (v) => v == null || v.isEmpty ? 'Unesite broj računa' : null,
                  onSaved: (v) => _bankAccountNumber = v ?? '',
                ),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Routing broj'),
                  validator: (v) => v == null || v.isEmpty ? 'Unesite routing broj' : null,
                  onSaved: (v) => _bankRoutingNumber = v ?? '',
                ),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Ime vlasnika računa'),
                  validator: (v) => v == null || v.isEmpty ? 'Unesite ime vlasnika' : null,
                  onSaved: (v) => _accountHolderName = v ?? '',
                ),
                TextFormField(
                  decoration: const InputDecoration(labelText: 'Naziv banke (opciono)'),
                  onSaved: (v) => _bankName = v,
                ),
              ],
              TextFormField(
                decoration: const InputDecoration(labelText: 'Opis (opciono)'),
                maxLength: 500,
                onSaved: (v) => _description = v,
              ),
              const SizedBox(height: 24),
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
                  : ElevatedButton(
                      onPressed: _submit,
                      child: const Text('Plati'),
                    ),
            ],
          ),
        ),
      ),
    );
  }
}