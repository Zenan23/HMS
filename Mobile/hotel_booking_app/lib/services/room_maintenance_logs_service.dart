import '../models/room_maintenance_log.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class RoomMaintenanceLogsService {
  Future<List<RoomMaintenanceLog>> getByRoomId(int roomId) async {
    final response = await ApiService.get('/RoomMaintenanceLogs/room/$roomId');
    return ApiResponseParser.parseList(response, RoomMaintenanceLog.fromJson);
  }
}
