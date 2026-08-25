import '../models/inventory_item_category.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class InventoryItemCategoryService {
  final _api = ApiService();

  Future<PaginatedResult<InventoryItemCategory>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/InventoryItemCategories?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(
        response, InventoryItemCategory.fromJson);
  }

  /// Za dropdown-e: dohvata veliku stranicu jer kategorija obično ima malo.
  Future<List<InventoryItemCategory>> getAllForDropdown() async {
    final result = await getPaged(pageNumber: 1, pageSize: 200);
    return result.items;
  }

  Future<InventoryItemCategory> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/InventoryItemCategories', body);
    return ApiResponseParser.parseObject(
        response, InventoryItemCategory.fromJson);
  }

  Future<InventoryItemCategory> update(
      int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/InventoryItemCategories/$id', body);
    return ApiResponseParser.parseObject(
        response, InventoryItemCategory.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/InventoryItemCategories/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
