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

  CreateHostedCheckoutDto({
    required this.userId,
    required this.bookingId,
    required this.amount,
    required this.paymentMethod,
    this.currency = 'EUR',
    this.description,
  });

  Map<String, dynamic> toJson() => {
        'userId': userId,
        'bookingId': bookingId,
        'amount': num.parse(amount.toStringAsFixed(2)),
        'paymentMethod': paymentMethod.apiValue,
        'currency': currency,
        if (description != null) 'description': description,
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
