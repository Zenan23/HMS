import '../models/inventory_item.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class InventoryItemService {
  final _api = ApiService();

  Future<PaginatedResult<InventoryItem>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/InventoryItems?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(response, InventoryItem.fromJson);
  }

  /// Za dropdown-e: dohvata veliku stranicu jer artikala obično ima malo.
  Future<List<InventoryItem>> getAllForDropdown() async {
    final result = await getPaged(pageNumber: 1, pageSize: 100);
    return result.items;
  }

  Future<InventoryItem> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/InventoryItems', body);
    return ApiResponseParser.parseObject(response, InventoryItem.fromJson);
  }

  Future<InventoryItem> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/InventoryItems/$id', body);
    return ApiResponseParser.parseObject(response, InventoryItem.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/InventoryItems/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
