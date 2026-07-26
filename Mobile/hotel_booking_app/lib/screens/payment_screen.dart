import 'dart:async';

import 'package:app_links/app_links.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';
import '../models/payment.dart';
import '../services/payments_service.dart';
import '../services/auth_service.dart';
import '../services/stripe_platform.dart';
import '../screens/home_screen.dart';
import '../utils/payment_deep_link.dart';
import '../widgets/paypal_checkout_webview.dart';
import '../widgets/stripe_payment_element.dart';

enum _PaymentPhase {
  idle,
  processing,
  browserOpen,
  paypalWebView,
  stripeElement,
  confirming,
  completed,
  failed,
}

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

class _PaymentScreenState extends State<PaymentScreen> with WidgetsBindingObserver {
  final _formKey = GlobalKey<FormState>();
  final _paymentsService = PaymentsService();
  PaymentMethod _method = PaymentMethod.stripe;
  String? _description;
  bool _loading = false;
  bool _configLoading = true;
  String? _error;
  _PaymentPhase _phase = _PaymentPhase.idle;
  int? _pendingPaymentId;
  String? _stripeClientSecret;
  String? _stripePaymentIntentId;
  bool _cardComplete = false;
  PaymentConfig? _config;
  bool _useNative = true;
  final AppLinks _appLinks = AppLinks();
  StreamSubscription<Uri>? _linkSub;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    unawaited(_loadConfig());
    _initDeepLinks();
  }

  Future<void> _loadConfig() async {
    final config = await _paymentsService.getPaymentConfig();
    if (!mounted) return;

    final enableNative = config?.enableNativeCheckout ?? true;
    final canUseInAppStripe = enableNative &&
        (config?.stripeConfigured ?? false) &&
        stripeCheckoutUi != StripeCheckoutUi.unsupported;

    setState(() {
      _config = config;
      // In-app Stripe (Payment Sheet / Payment Element). PayPal native samo van weba.
      _useNative = canUseInAppStripe || (enableNative && !kIsWeb && (config?.payPalConfigured ?? false));
      _configLoading = false;
      if (config != null && !config.stripeConfigured && config.payPalConfigured) {
        _method = PaymentMethod.paypal;
      }
    });

    if (canUseInAppStripe &&
        config?.stripePublishableKey != null &&
        config!.stripePublishableKey!.isNotEmpty) {
      await configureStripePublishableKey(config.stripePublishableKey!);
    }
  }

  Future<void> _initDeepLinks() async {
    final initial = await _appLinks.getInitialLink();
    if (initial != null) {
      await _handlePaymentReturnUri(initial);
    }
    _linkSub = _appLinks.uriLinkStream.listen((uri) {
      unawaited(_handlePaymentReturnUri(uri));
    });
  }

  Future<void> _handlePaymentReturnUri(Uri uri) async {
    if (_useNative) return;
    final params = PaymentReturnParams.tryParse(uri);
    if (params == null || !mounted) return;

    if (params.isCancel) {
      setState(() {
        _phase = _PaymentPhase.failed;
        _error = 'Plaćanje je otkazano.';
        _loading = false;
      });
      return;
    }

    final paymentId = params.paymentId ?? _pendingPaymentId;
    if (paymentId != null) _pendingPaymentId = paymentId;

    setState(() {
      _phase = _PaymentPhase.confirming;
      _error = null;
      _loading = false;
    });

    if (params.sessionId != null && params.sessionId!.isNotEmpty) {
      _method = PaymentMethod.stripe;
      await _paymentsService.finalizeStripeSession(params.sessionId!);
    } else if (params.payPalToken != null && params.payPalToken!.isNotEmpty) {
      _method = PaymentMethod.paypal;
      await _paymentsService.capturePayPalOrder(params.payPalToken!);
    }

    if (paymentId != null && mounted) {
      await _waitForCompletion(paymentId, _method, tryProviderConfirm: true);
    }
  }

  @override
  void dispose() {
    _linkSub?.cancel();
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (!_useNative &&
        state == AppLifecycleState.resumed &&
        _pendingPaymentId != null &&
        _phase == _PaymentPhase.browserOpen) {
      _onReturnedFromBrowser();
    }
  }

  CreateHostedCheckoutDto _buildCheckoutDto(int userId) {
    // PayPal sandbox često odbija EUR (COMPLIANCE_VIOLATION) — koristi USD za PayPal.
    final currency = _method == PaymentMethod.paypal ? 'USD' : widget.currency;
    // PayPal odbija privatne IP (10.0.2.2). Koristi javni HTTPS host — WebView ga
    // presreće prije stvarnog mrežnog učitavanja (vidi payment_deep_link.dart).
    const payPalReturnHost = 'https://ebooking.app';
    return CreateHostedCheckoutDto(
      userId: userId,
      bookingId: widget.bookingId,
      amount: widget.amount,
      paymentMethod: _method,
      currency: currency,
      description: _description,
      returnUrl: _method == PaymentMethod.paypal ? '$payPalReturnHost/payment-return' : null,
      cancelUrl: _method == PaymentMethod.paypal ? '$payPalReturnHost/payment-cancel' : null,
    );
  }

  Future<void> _startPayment() async {
    if (!(_formKey.currentState?.validate() ?? true)) return;
    _formKey.currentState?.save();

    final userId = Provider.of<AuthService>(context, listen: false).user?.userId;
    if (userId == null) {
      setState(() => _error = 'Niste prijavljeni.');
      return;
    }

    if (_useNative) {
      if (_method == PaymentMethod.stripe) {
        await _startStripeNative(userId);
      } else if (kIsWeb) {
        // PayPal WebView nije pouzdan na Flutter web — hosted redirect.
        await _startHostedCheckout(userId);
      } else {
        await _startPayPalNative(userId);
      }
    } else {
      await _startHostedCheckout(userId);
    }
  }

  Future<void> _startStripeNative(int userId) async {
    if (_config?.stripeConfigured != true) {
      setState(() => _error = 'Stripe nije konfigurisan na serveru (SecretKey + PublishableKey).');
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
      _phase = _PaymentPhase.processing;
    });

    final intent = await _paymentsService.startStripeIntent(_buildCheckoutDto(userId));
    if (intent == null) {
      setState(() {
        _loading = false;
        _phase = _PaymentPhase.failed;
        _error = 'Neuspješno kreiranje Stripe plaćanja.';
      });
      return;
    }

    _pendingPaymentId = intent.paymentId;
    _stripePaymentIntentId = intent.paymentIntentId;
    _stripeClientSecret = intent.clientSecret;

    // Web: ugrađeni Payment Element. Mobile: Payment Sheet.
    if (stripeCheckoutUi == StripeCheckoutUi.paymentElement) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _cardComplete = false;
        _phase = _PaymentPhase.stripeElement;
      });
      return;
    }

    try {
      await presentStripePaymentSheet(
        clientSecret: intent.clientSecret,
        merchantDisplayName: 'Hotel Booking',
      );
    } catch (e) {
      if (!mounted) return;
      final msg = e.toString();
      final cancelled = msg.contains('Canceled') || msg.contains('cancelled');
      setState(() {
        _loading = false;
        _phase = _PaymentPhase.failed;
        _error = cancelled ? 'Plaćanje je otkazano.' : 'Stripe plaćanje nije uspjelo.';
      });
      return;
    }

    if (!mounted) return;
    setState(() {
      _loading = false;
      _phase = _PaymentPhase.confirming;
    });

    final confirmed = await _paymentsService.confirmStripePaymentIntent(intent.paymentIntentId);
    if (!mounted) return;
    if (!confirmed && !await _paymentsService.isPaymentCompleted(intent.paymentId)) {
      setState(() {
        _phase = _PaymentPhase.failed;
        _error =
            'Stripe nije potvrdio uplatu (kartica nije obrađena). '
            'Zatvorite sheet tek nakon uspješnog plaćanja, ili pokušajte ponovo s test karticom 4242…';
      });
      return;
    }

    await _waitForCompletion(intent.paymentId, PaymentMethod.stripe, tryProviderConfirm: true);
  }

  Future<void> _confirmStripeElement() async {
    final paymentId = _pendingPaymentId;
    final intentId = _stripePaymentIntentId;
    if (paymentId == null || intentId == null) return;

    setState(() {
      _loading = true;
      _error = null;
      _phase = _PaymentPhase.confirming;
    });

    try {
      await confirmWebPaymentElement(returnUrl: currentPageUrl());
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _loading = false;
        _phase = _PaymentPhase.stripeElement;
        _error = 'Stripe plaćanje nije uspjelo. Provjerite podatke kartice.';
      });
      return;
    }

    await _paymentsService.confirmStripePaymentIntent(intentId);
    if (!mounted) return;
    await _waitForCompletion(paymentId, PaymentMethod.stripe, tryProviderConfirm: true);
  }

  Future<void> _startPayPalNative(int userId) async {
    if (_config?.payPalConfigured != true) {
      setState(() => _error = 'PayPal nije konfigurisan na serveru.');
      return;
    }

    setState(() {
      _loading = true;
      _error = null;
      _phase = _PaymentPhase.processing;
    });

    final order = await _paymentsService.startPayPalOrder(_buildCheckoutDto(userId));
    if (order == null) {
      setState(() {
        _loading = false;
        _phase = _PaymentPhase.failed;
        _error = 'Neuspješno kreiranje PayPal narudžbe.';
      });
      return;
    }

    _pendingPaymentId = order.paymentId;

    if (!mounted) return;
    setState(() {
      _loading = false;
      _phase = _PaymentPhase.paypalWebView;
    });

    await Navigator.of(context).push<void>(
      MaterialPageRoute(
        fullscreenDialog: true,
        builder: (_) => PayPalCheckoutWebView(
          approveUrl: order.approveUrl,
          onCancel: () {
            if (mounted) {
              setState(() {
                _phase = _PaymentPhase.failed;
                _error = 'PayPal plaćanje je otkazano.';
              });
            }
          },
          onReturn: (params) async {
            if (params.isCancel) {
              if (mounted) {
                setState(() {
                  _phase = _PaymentPhase.failed;
                  _error = 'PayPal plaćanje je otkazano.';
                });
              }
              return;
            }

            final token = params.payPalToken ?? order.orderId;
            if (!mounted) return;
            setState(() {
              _phase = _PaymentPhase.confirming;
              _error = null;
            });
            final captureError = await _paymentsService.capturePayPalOrder(token);
            if (!mounted) return;
            if (captureError != null) {
              final friendly = captureError.contains('COMPLIANCE_VIOLATION')
                  ? 'PayPal sandbox je odbio transakciju (COMPLIANCE_VIOLATION).\n\n'
                      'U PayPal Developer Dashboardu kreiraj novi Sandbox Business (US) nalog, '
                      'poveži REST app credentials u .env, i plaćaj Sandbox Personal (US) buyer nalogom. '
                      'Valuta mora biti USD.'
                  : captureError;
              setState(() {
                _phase = _PaymentPhase.failed;
                _error = friendly;
              });
              return;
            }
            await _waitForCompletion(order.paymentId, PaymentMethod.paypal, tryProviderConfirm: true);
          },
        ),
      ),
    );
  }

  Future<void> _startHostedCheckout(int userId) async {
    setState(() {
      _loading = true;
      _error = null;
      _phase = _PaymentPhase.idle;
    });

    final checkout = await _paymentsService.startHostedCheckout(_buildCheckoutDto(userId));
    if (checkout == null) {
      setState(() {
        _loading = false;
        _error = 'Neuspješno kreiranje plaćanja. Provjerite API i Stripe/PayPal ključeve.';
      });
      return;
    }

    _pendingPaymentId = checkout.paymentId;
    final uri = Uri.parse(checkout.redirectUrl);
    final launched = await launchUrl(uri, mode: LaunchMode.externalApplication);
    if (!launched) {
      setState(() {
        _loading = false;
        _error = 'Nije moguće otvoriti stranicu za plaćanje.';
      });
      return;
    }

    setState(() {
      _loading = false;
      _phase = _PaymentPhase.browserOpen;
    });

    await _waitForCompletion(checkout.paymentId, _method);
  }

  Future<void> _onReturnedFromBrowser() async {
    final id = _pendingPaymentId;
    if (id == null || !mounted) return;
    setState(() {
      _phase = _PaymentPhase.confirming;
      _error = null;
    });
    await _waitForCompletion(id, _method, tryProviderConfirm: true);
  }

  Future<void> _waitForCompletion(
    int paymentId,
    PaymentMethod method, {
    bool tryProviderConfirm = false,
  }) async {
    if (tryProviderConfirm) {
      setState(() => _phase = _PaymentPhase.confirming);
      await _paymentsService.confirmPaymentAfterReturn(paymentId, method);
      if (!mounted) return;
      if (await _paymentsService.isPaymentCompleted(paymentId)) {
        await _completeSuccess();
        return;
      }
    }

    final ok = await _paymentsService.waitForPaymentCompletion(
      paymentId,
      method: method,
    );

    if (!mounted) return;
    if (ok) {
      await _completeSuccess();
      return;
    }

    setState(() {
      _phase = _PaymentPhase.failed;
      _error = 'Plaćanje još nije potvrđeno. Dodirnite „Provjeri status”.';
    });
  }

  Future<void> _completeSuccess() async {
    if (!mounted) return;
    setState(() => _phase = _PaymentPhase.completed);
    await Future.delayed(const Duration(milliseconds: 800));
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const HomeScreen(initialTabIndex: 1)),
      (route) => false,
    );
  }

  Future<void> _retryConfirm() async {
    final id = _pendingPaymentId;
    if (id == null) return;
    setState(() {
      _loading = true;
      _error = null;
      _phase = _PaymentPhase.confirming;
    });
    await _paymentsService.confirmPaymentAfterReturn(id, _method);
    if (!mounted) return;
    if (await _paymentsService.isPaymentCompleted(id)) {
      setState(() => _loading = false);
      await _completeSuccess();
      return;
    }
    setState(() {
      _loading = false;
      _phase = _PaymentPhase.failed;
      _error = 'Status još nije „završeno”. Pokušajte ponovo za nekoliko sekundi.';
    });
  }

  String get _providerLabel => _method == PaymentMethod.stripe ? 'Stripe' : 'PayPal';

  bool get _stripeAvailable => _config?.stripeConfigured ?? false;
  bool get _payPalAvailable => _config?.payPalConfigured ?? false;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final inBrowser = _phase == _PaymentPhase.browserOpen;
    final showingElement = _phase == _PaymentPhase.stripeElement;
    final confirming = _phase == _PaymentPhase.confirming || (_loading && !showingElement);
    final canPay = _phase == _PaymentPhase.idle && !_loading && !_configLoading;

    return Scaffold(
      appBar: AppBar(title: const Text('Plaćanje')),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: _configLoading
            ? const Center(child: CircularProgressIndicator())
            : Form(
                key: _formKey,
                child: ListView(
                  children: [
                    Text(
                      '${widget.amount.toStringAsFixed(2)} ${widget.currency}',
                      style: theme.textTheme.headlineMedium?.copyWith(fontWeight: FontWeight.bold),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Rezervacija #${widget.bookingId}',
                      style: theme.textTheme.bodyMedium?.copyWith(color: theme.hintColor),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 28),
                    if (_phase == _PaymentPhase.idle) ...[
                      Row(
                        children: [
                          Expanded(
                            child: _MethodTile(
                              selected: _method == PaymentMethod.stripe,
                              enabled: _stripeAvailable || !_useNative,
                              icon: Icons.credit_card,
                              label: 'Kartica',
                              color: const Color(0xFF635BFF),
                              onTap: () => setState(() => _method = PaymentMethod.stripe),
                            ),
                          ),
                          const SizedBox(width: 12),
                          Expanded(
                            child: _MethodTile(
                              selected: _method == PaymentMethod.paypal,
                              enabled: _payPalAvailable || !_useNative,
                              icon: Icons.account_balance_wallet_outlined,
                              label: 'PayPal',
                              color: const Color(0xFF003087),
                              onTap: () => setState(() => _method = PaymentMethod.paypal),
                            ),
                          ),
                        ],
                      ),
                      if (_useNative && !_stripeAvailable && !_payPalAvailable)
                        Padding(
                          padding: const EdgeInsets.only(top: 12),
                          child: Text(
                            'Plaćanje nije konfigurisano.',
                            style: TextStyle(color: theme.colorScheme.error, fontSize: 13),
                            textAlign: TextAlign.center,
                          ),
                        ),
                    ],
                    if (showingElement && _stripeClientSecret != null) ...[
                      SizedBox(
                        height: 300,
                        child: StripePaymentElementView(
                          clientSecret: _stripeClientSecret!,
                          onCardComplete: (complete) {
                            if (mounted) setState(() => _cardComplete = complete);
                          },
                        ),
                      ),
                      const SizedBox(height: 16),
                      FilledButton(
                        onPressed: (_loading || !_cardComplete) ? null : _confirmStripeElement,
                        child: Text('Plati ${widget.amount.toStringAsFixed(2)} ${widget.currency}'),
                      ),
                    ],
                    if (inBrowser || confirming) ...[
                      const SizedBox(height: 32),
                      const Center(child: CircularProgressIndicator()),
                      const SizedBox(height: 16),
                      Center(
                        child: Text(
                          inBrowser ? 'Završite plaćanje…' : 'Provjeravamo plaćanje…',
                          style: theme.textTheme.bodyMedium,
                        ),
                      ),
                    ],
                    if (_phase == _PaymentPhase.completed) ...[
                      const SizedBox(height: 32),
                      const Icon(Icons.check_circle, color: Colors.green, size: 56),
                      const SizedBox(height: 8),
                      const Center(child: Text('Plaćanje uspješno')),
                    ],
                    if (_error != null) ...[
                      const SizedBox(height: 16),
                      Text(
                        _error!,
                        style: TextStyle(color: theme.colorScheme.error),
                        textAlign: TextAlign.center,
                      ),
                    ],
                    const SizedBox(height: 24),
                    if (canPay)
                      FilledButton(
                        onPressed: _startPayment,
                        child: Text('Plati $_providerLabel'),
                      ),
                    if (inBrowser || _phase == _PaymentPhase.failed) ...[
                      OutlinedButton(
                        onPressed: _loading ? null : _retryConfirm,
                        child: const Text('Provjeri status'),
                      ),
                    ],
                  ],
                ),
              ),
      ),
    );
  }
}

class _MethodTile extends StatelessWidget {
  final bool selected;
  final bool enabled;
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;

  const _MethodTile({
    required this.selected,
    required this.enabled,
    required this.icon,
    required this.label,
    required this.color,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final borderColor = selected ? color : theme.dividerColor;
    final fg = enabled ? (selected ? color : theme.colorScheme.onSurface) : theme.disabledColor;

    return Material(
      color: selected ? color.withValues(alpha: 0.08) : theme.cardColor,
      borderRadius: BorderRadius.circular(16),
      child: InkWell(
        onTap: enabled ? onTap : null,
        borderRadius: BorderRadius.circular(16),
        child: Container(
          padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 12),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: borderColor, width: selected ? 2 : 1),
          ),
          child: Column(
            children: [
              Icon(icon, size: 36, color: fg),
              const SizedBox(height: 10),
              Text(
                label,
                style: theme.textTheme.titleSmall?.copyWith(
                  color: fg,
                  fontWeight: selected ? FontWeight.w700 : FontWeight.w500,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
