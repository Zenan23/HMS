import '../models/city.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class CityService {
  final _api = ApiService();

  Future<PaginatedResult<City>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response =
        await _api.get('/api/Cities?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(response, City.fromJson);
  }

  /// Za dropdown-e: dohvata veliku stranicu jer gradova obično ima malo.
  Future<List<City>> getAllForDropdown() async {
    final result = await getPaged(pageNumber: 1, pageSize: 100);
    return result.items;
  }

  Future<City> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/Cities', body);
    return ApiResponseParser.parseObject(response, City.fromJson);
  }

  Future<City> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/Cities/$id', body);
    return ApiResponseParser.parseObject(response, City.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/Cities/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
