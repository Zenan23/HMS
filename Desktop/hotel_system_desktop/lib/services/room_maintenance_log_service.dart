import '../models/room_maintenance_log.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class RoomMaintenanceLogService {
  final _api = ApiService();

  Future<PaginatedResult<RoomMaintenanceLog>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/RoomMaintenanceLogs?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(
        response, RoomMaintenanceLog.fromJson);
  }

  Future<List<RoomMaintenanceLog>> getByRoomId(int roomId) async {
    final response = await _api.get('/api/RoomMaintenanceLogs/room/$roomId');
    return ApiResponseParser.parseList(response, RoomMaintenanceLog.fromJson);
  }

  Future<RoomMaintenanceLog> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/RoomMaintenanceLogs', body);
    return ApiResponseParser.parseObject(response, RoomMaintenanceLog.fromJson);
  }

  Future<RoomMaintenanceLog> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/RoomMaintenanceLogs/$id', body);
    return ApiResponseParser.parseObject(response, RoomMaintenanceLog.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/RoomMaintenanceLogs/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
