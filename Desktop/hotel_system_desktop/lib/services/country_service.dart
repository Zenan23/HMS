import '../models/country.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class CountryService {
  final _api = ApiService();

  Future<PaginatedResult<Country>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api
        .get('/api/Countries?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(response, Country.fromJson);
  }

  /// Za dropdown-e: dohvata veliku stranicu jer država obično ima malo.
  Future<List<Country>> getAllForDropdown() async {
    final result = await getPaged(pageNumber: 1, pageSize: 100);
    return result.items;
  }

  Future<Country> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/Countries', body);
    return ApiResponseParser.parseObject(response, Country.fromJson);
  }

  Future<Country> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/Countries/$id', body);
    return ApiResponseParser.parseObject(response, Country.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/Countries/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
