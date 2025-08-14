enum PaymentMethod { card, paypal, bankTransfer }
// Card = 1, PayPal = 2, BankTransfer = 3

extension PaymentMethodExt on PaymentMethod {
  int get apiValue {
    switch (this) {
      case PaymentMethod.card:
        return 1;
      case PaymentMethod.paypal:
        return 2;
      case PaymentMethod.bankTransfer:
        return 3;
    }
  }
}

enum PaymentStatus { pending, completed, failed, refunded }

class CardPaymentData {
  final String cardNumber;
  final int expiryMonth;
  final int expiryYear;
  final String cvv;
  final String cardholderName;

  CardPaymentData({
    required this.cardNumber,
    required this.expiryMonth,
    required this.expiryYear,
    required this.cvv,
    required this.cardholderName,
  });

  Map<String, dynamic> toJson() => {
    'cardNumber': cardNumber,
    'expiryMonth': expiryMonth,
    'expiryYear': expiryYear,
    'cvv': cvv,
    'cardholderName': cardholderName,
  };
}

class PayPalPaymentData {
  final String payPalEmail;
  PayPalPaymentData({required this.payPalEmail});
  Map<String, dynamic> toJson() => {'payPalEmail': payPalEmail};
}

class BankTransferPaymentData {
  final String bankAccountNumber;
  final String bankRoutingNumber;
  final String accountHolderName;
  final String? bankName;

  BankTransferPaymentData({
    required this.bankAccountNumber,
    required this.bankRoutingNumber,
    required this.accountHolderName,
    this.bankName,
  });

  Map<String, dynamic> toJson() => {
    'bankAccountNumber': bankAccountNumber,
    'bankRoutingNumber': bankRoutingNumber,
    'accountHolderName': accountHolderName,
    if (bankName != null) 'bankName': bankName,
  };
}

class CreatePaymentDto {
  final int userId;
  final int bookingId;
  final num amount;
  final PaymentMethod paymentMethod;
  final String currency;
  final String? description;
  final CardPaymentData? cardData;
  final PayPalPaymentData? payPalData;
  final BankTransferPaymentData? bankTransferData;

  CreatePaymentDto({
    required this.userId,
    required this.bookingId,
    required this.amount,
    required this.paymentMethod,
    this.currency = 'USD',
    this.description,
    this.cardData,
    this.payPalData,
    this.bankTransferData,
  });

  Map<String, dynamic> toJson() => {
    'userId': userId,
    'bookingId': bookingId,
    'amount': num.parse(amount.toStringAsFixed(2)),
    'paymentMethod': paymentMethod.apiValue,
    'currency': currency,
    if (description != null) 'description': description,
    if (cardData != null) 'cardData': cardData!.toJson(),
    if (payPalData != null) 'payPalData': payPalData!.toJson(),
    if (bankTransferData != null) 'bankTransferData': bankTransferData!.toJson(),
  };
}