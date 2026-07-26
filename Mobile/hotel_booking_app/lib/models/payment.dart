/// Usklađeno sa backend [Contracts.Enums.PaymentMethod]
enum PaymentMethod {
  paypal,
  stripe,
}

extension PaymentMethodExt on PaymentMethod {
  int get apiValue {
    switch (this) {
      case PaymentMethod.paypal:
        return 2;
      case PaymentMethod.stripe:
        return 4;
    }
  }
}

class CreateHostedCheckoutDto {
  final int userId;
  final int bookingId;
  final num amount;
  final PaymentMethod paymentMethod;
  final String currency;
  final String? description;
  final String? returnUrl;
  final String? cancelUrl;

  CreateHostedCheckoutDto({
    required this.userId,
    required this.bookingId,
    required this.amount,
    required this.paymentMethod,
    this.currency = 'EUR',
    this.description,
    this.returnUrl,
    this.cancelUrl,
  });

  Map<String, dynamic> toJson() => {
        'userId': userId,
        'bookingId': bookingId,
        'amount': num.parse(amount.toStringAsFixed(2)),
        'paymentMethod': paymentMethod.apiValue,
        'currency': currency,
        if (description != null) 'description': description,
        if (returnUrl != null) 'returnUrl': returnUrl,
        if (cancelUrl != null) 'cancelUrl': cancelUrl,
      };
}

class HostedCheckoutResponse {
  final int paymentId;
  final String redirectUrl;
  final int paymentMethod;

  HostedCheckoutResponse({
    required this.paymentId,
    required this.redirectUrl,
    required this.paymentMethod,
  });

  factory HostedCheckoutResponse.fromJson(Map<String, dynamic> json) {
    return HostedCheckoutResponse(
      paymentId: (json['paymentId'] as num).toInt(),
      redirectUrl: json['redirectUrl'] as String,
      paymentMethod: (json['paymentMethod'] as num).toInt(),
    );
  }
}

class PaymentConfig {
  final bool enableNativeCheckout;
  final bool useHostedCheckout;
  final String? stripePublishableKey;
  final bool stripeConfigured;
  final bool payPalConfigured;

  PaymentConfig({
    required this.enableNativeCheckout,
    required this.useHostedCheckout,
    this.stripePublishableKey,
    required this.stripeConfigured,
    required this.payPalConfigured,
  });

  factory PaymentConfig.fromJson(Map<String, dynamic> json) {
    return PaymentConfig(
      enableNativeCheckout: json['enableNativeCheckout'] as bool? ?? true,
      useHostedCheckout: json['useHostedCheckout'] as bool? ?? true,
      stripePublishableKey: json['stripePublishableKey'] as String?,
      stripeConfigured: json['stripeConfigured'] as bool? ?? false,
      payPalConfigured: json['payPalConfigured'] as bool? ?? false,
    );
  }
}

class StripeIntentResponse {
  final int paymentId;
  final String clientSecret;
  final String paymentIntentId;
  final String currency;

  StripeIntentResponse({
    required this.paymentId,
    required this.clientSecret,
    required this.paymentIntentId,
    required this.currency,
  });

  factory StripeIntentResponse.fromJson(Map<String, dynamic> json) {
    return StripeIntentResponse(
      paymentId: (json['paymentId'] as num).toInt(),
      clientSecret: json['clientSecret'] as String,
      paymentIntentId: json['paymentIntentId'] as String,
      currency: json['currency'] as String? ?? 'EUR',
    );
  }
}

class PayPalNativeOrderResponse {
  final int paymentId;
  final String orderId;
  final String approveUrl;

  PayPalNativeOrderResponse({
    required this.paymentId,
    required this.orderId,
    required this.approveUrl,
  });

  factory PayPalNativeOrderResponse.fromJson(Map<String, dynamic> json) {
    return PayPalNativeOrderResponse(
      paymentId: (json['paymentId'] as num).toInt(),
      orderId: json['orderId'] as String,
      approveUrl: json['approveUrl'] as String,
    );
  }
}

/// Payment status sa API-ja (Contracts.Enums.PaymentStatus)
enum PaymentStatusApi {
  pending(1),
  processing(2),
  completed(3),
  failed(4),
  cancelled(5),
  refunded(6),
  partiallyRefunded(7);

  final int value;
  const PaymentStatusApi(this.value);

  static PaymentStatusApi? fromInt(int? v) {
    if (v == null) return null;
    for (final s in PaymentStatusApi.values) {
      if (s.value == v) return s;
    }
    return null;
  }
}

/// Detalji plaćanja sa GET /Payments/{id} (za polling i potvrdu nakon checkout-a).
class PaymentDetails {
  final int id;
  final PaymentStatusApi? status;
  final PaymentMethod? paymentMethod;
  final String? checkoutId;

  PaymentDetails({
    required this.id,
    this.status,
    this.paymentMethod,
    this.checkoutId,
  });

  factory PaymentDetails.fromJson(Map<String, dynamic> json) {
    final methodRaw = (json['paymentMethod'] as num?)?.toInt();
    PaymentMethod? method;
    if (methodRaw == 2) {
      method = PaymentMethod.paypal;
    } else if (methodRaw == 4) {
      method = PaymentMethod.stripe;
    }
    return PaymentDetails(
      id: (json['id'] as num).toInt(),
      status: PaymentStatusApi.fromInt((json['status'] as num?)?.toInt()),
      paymentMethod: method,
      checkoutId: json['checkoutId'] as String?,
    );
  }

  bool get isCompleted => status == PaymentStatusApi.completed;
  bool get isPendingConfirmation =>
      status == PaymentStatusApi.processing || status == PaymentStatusApi.pending;
}
