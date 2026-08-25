import '../models/service_category.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class ServiceCategoryService {
  final _api = ApiService();

  Future<PaginatedResult<ServiceCategory>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/ServiceCategories?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(
        response, ServiceCategory.fromJson);
  }

  /// Za dropdown-e: dohvata veliku stranicu jer kategorija obično ima malo.
  Future<List<ServiceCategory>> getAllForDropdown() async {
    final result = await getPaged(pageNumber: 1, pageSize: 200);
    return result.items;
  }

  Future<ServiceCategory> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/ServiceCategories', body);
    return ApiResponseParser.parseObject(response, ServiceCategory.fromJson);
  }

  Future<ServiceCategory> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/ServiceCategories/$id', body);
    return ApiResponseParser.parseObject(response, ServiceCategory.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/ServiceCategories/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
