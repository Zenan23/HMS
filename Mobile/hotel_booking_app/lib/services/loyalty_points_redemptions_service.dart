import '../models/loyalty_points_redemption.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class LoyaltyPointsRedemptionsService {
  Future<List<LoyaltyPointsRedemption>> getByUserId(int userId) async {
    final response =
        await ApiService.get('/LoyaltyPointsRedemptions/user/$userId');
    return ApiResponseParser.parseList(response, LoyaltyPointsRedemption.fromJson);
  }

  Future<List<LoyaltyPointsRedemption>> getByBookingId(int bookingId) async {
    final response =
        await ApiService.get('/LoyaltyPointsRedemptions/booking/$bookingId');
    return ApiResponseParser.parseList(response, LoyaltyPointsRedemption.fromJson);
  }

  Future<int> getBalance(int userId) async {
    final response =
        await ApiService.get('/LoyaltyPointsRedemptions/balance/$userId');
    final data = ApiResponseParser.extractData(response);
    return data is int ? data : int.tryParse(data.toString()) ?? 0;
  }

  /// Gost sam kreira redemption za svoju rezervaciju — backend provjerava vlasništvo i balans,
  /// EquivalentValueAmount se uvijek računa server-side (klijent ga ne šalje).
  Future<LoyaltyPointsRedemption> redeem({
    required int userId,
    required int bookingId,
    required int pointsUsed,
  }) async {
    final response = await ApiService.post('/LoyaltyPointsRedemptions', {
      'userId': userId,
      'bookingId': bookingId,
      'pointsUsed': pointsUsed,
      'redeemedAt': DateTime.now().toUtc().toIso8601String(),
      'equivalentValueAmount': 0,
    });
    return ApiResponseParser.parseObject(response, LoyaltyPointsRedemption.fromJson);
  }
}
