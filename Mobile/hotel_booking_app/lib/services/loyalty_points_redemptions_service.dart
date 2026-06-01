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
}
