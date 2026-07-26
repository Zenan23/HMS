import 'package:flutter/material.dart';
import 'package:webview_flutter/webview_flutter.dart';
import '../utils/payment_deep_link.dart';

/// In-app PayPal odobrenje — WebView hvata HTTP return i ebooking:// deep link.
class PayPalCheckoutWebView extends StatefulWidget {
  final String approveUrl;
  final void Function(PaymentReturnParams params) onReturn;
  final VoidCallback? onCancel;

  const PayPalCheckoutWebView({
    super.key,
    required this.approveUrl,
    required this.onReturn,
    this.onCancel,
  });

  @override
  State<PayPalCheckoutWebView> createState() => _PayPalCheckoutWebViewState();
}

class _PayPalCheckoutWebViewState extends State<PayPalCheckoutWebView> {
  late final WebViewController _controller;
  bool _handled = false;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _controller = WebViewController()
      ..setJavaScriptMode(JavaScriptMode.unrestricted)
      ..setNavigationDelegate(
        NavigationDelegate(
          onPageStarted: (url) {
            _tryHandleUrl(url);
            if (mounted) setState(() => _loading = true);
          },
          onPageFinished: (url) {
            _tryHandleUrl(url);
            if (mounted) setState(() => _loading = false);
          },
          onNavigationRequest: (request) {
            if (_tryHandleUrl(request.url)) {
              return NavigationDecision.prevent;
            }
            // Blokiraj custom scheme navigaciju (WebView crash / genericError) —
            // već obrađeno u tryParse ako je ebooking://
            final uri = Uri.tryParse(request.url);
            if (uri != null &&
                uri.scheme != 'http' &&
                uri.scheme != 'https' &&
                uri.scheme != 'about' &&
                uri.scheme != 'data') {
              _tryHandleUrl(request.url);
              return NavigationDecision.prevent;
            }
            return NavigationDecision.navigate;
          },
          onUrlChange: (change) {
            final url = change.url;
            if (url != null) _tryHandleUrl(url);
          },
        ),
      )
      ..loadRequest(Uri.parse(widget.approveUrl));
  }

  bool _tryHandleUrl(String url) {
    if (_handled) return true;
    final uri = Uri.tryParse(url);
    if (uri == null) return false;
    final params = PaymentReturnParams.tryParse(uri);
    if (params == null) return false;
    _finish(params);
    return true;
  }

  void _finish(PaymentReturnParams params) {
    if (_handled) return;
    _handled = true;
    widget.onReturn(params);
    if (mounted) Navigator.of(context).pop();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('PayPal'),
        leading: IconButton(
          icon: const Icon(Icons.close),
          onPressed: () {
            widget.onCancel?.call();
            Navigator.of(context).pop();
          },
        ),
      ),
      body: Stack(
        children: [
          WebViewWidget(controller: _controller),
          if (_loading) const Center(child: CircularProgressIndicator()),
        ],
      ),
    );
  }
}
