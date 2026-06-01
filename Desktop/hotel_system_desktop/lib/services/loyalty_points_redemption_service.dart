import '../models/loyalty_points_redemption.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class LoyaltyPointsRedemptionService {
  final _api = ApiService();

  Future<PaginatedResult<LoyaltyPointsRedemption>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/LoyaltyPointsRedemptions?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(
        response, LoyaltyPointsRedemption.fromJson);
  }

  Future<List<LoyaltyPointsRedemption>> getByUserId(int userId) async {
    final response =
        await _api.get('/api/LoyaltyPointsRedemptions/user/$userId');
    return ApiResponseParser.parseList(
        response, LoyaltyPointsRedemption.fromJson);
  }

  Future<List<LoyaltyPointsRedemption>> getByBookingId(int bookingId) async {
    final response =
        await _api.get('/api/LoyaltyPointsRedemptions/booking/$bookingId');
    return ApiResponseParser.parseList(
        response, LoyaltyPointsRedemption.fromJson);
  }

  Future<LoyaltyPointsRedemption> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/LoyaltyPointsRedemptions', body);
    return ApiResponseParser.parseObject(
        response, LoyaltyPointsRedemption.fromJson);
  }

  Future<LoyaltyPointsRedemption> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/LoyaltyPointsRedemptions/$id', body);
    return ApiResponseParser.parseObject(
        response, LoyaltyPointsRedemption.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/LoyaltyPointsRedemptions/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
