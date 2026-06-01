import '../models/inventory_transaction.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class InventoryTransactionsService {
  Future<List<InventoryTransaction>> getByInventoryItemId(int itemId) async {
    final response =
        await ApiService.get('/InventoryTransactions/item/$itemId');
    return ApiResponseParser.parseList(response, InventoryTransaction.fromJson);
  }
}
