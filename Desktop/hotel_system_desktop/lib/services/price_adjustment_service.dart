import '../models/price_adjustment.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class PriceAdjustmentService {
  final _api = ApiService();

  Future<PaginatedResult<PriceAdjustment>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/PriceAdjustments?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(response, PriceAdjustment.fromJson);
  }

  Future<List<PriceAdjustment>> getActive({DateTime? atDate}) async {
    final date = (atDate ?? DateTime.now()).toUtc().toIso8601String();
    final response =
        await _api.get('/api/PriceAdjustments/active?atDate=$date');
    return ApiResponseParser.parseList(response, PriceAdjustment.fromJson);
  }

  Future<PriceAdjustment> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/PriceAdjustments', body);
    return ApiResponseParser.parseObject(response, PriceAdjustment.fromJson);
  }

  Future<PriceAdjustment> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/PriceAdjustments/$id', body);
    return ApiResponseParser.parseObject(response, PriceAdjustment.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/PriceAdjustments/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
