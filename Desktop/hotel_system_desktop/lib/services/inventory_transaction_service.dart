import '../models/inventory_transaction.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class InventoryTransactionService {
  final _api = ApiService();

  Future<PaginatedResult<InventoryTransaction>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/InventoryTransactions?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(
        response, InventoryTransaction.fromJson);
  }

  Future<List<InventoryTransaction>> getByInventoryItemId(int itemId) async {
    final response =
        await _api.get('/api/InventoryTransactions/item/$itemId');
    return ApiResponseParser.parseList(response, InventoryTransaction.fromJson);
  }

  Future<List<InventoryTransaction>> getByStaffUserId(int staffUserId) async {
    final response =
        await _api.get('/api/InventoryTransactions/staff/$staffUserId');
    return ApiResponseParser.parseList(response, InventoryTransaction.fromJson);
  }

  Future<InventoryTransaction> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/InventoryTransactions', body);
    return ApiResponseParser.parseObject(
        response, InventoryTransaction.fromJson);
  }

  Future<InventoryTransaction> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/InventoryTransactions/$id', body);
    return ApiResponseParser.parseObject(
        response, InventoryTransaction.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/InventoryTransactions/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
