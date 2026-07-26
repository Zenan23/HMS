import '../models/room.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class RoomsService {
  Future<List<Room>> fetchRooms(
      {int page = 1, int pageSize = 10, String? filter}) async {
    final query =
        '?pageNumber=$page&pageSize=$pageSize${filter != null ? '&filter=$filter' : ''}';
    final response = await ApiService.get('/Rooms$query');
    final result = ApiResponseParser.parsePaginated(response, Room.fromJson);
    return result.items;
  }

  Future<bool> checkAvailability(
      int roomId, String checkIn, String checkOut, {String? services}) async {
    final svcQuery = (services != null && services.isNotEmpty)
        ? '&services=${Uri.encodeQueryComponent(services)}'
        : '';
    final response = await ApiService.get(
        '/Rooms/$roomId/availability?checkIn=$checkIn&checkOut=$checkOut$svcQuery');
    final data = ApiResponseParser.extractData(response);
    return data == true;
  }

  Future<double> calculatePrice(
      int roomId, String checkIn, String checkOut, int guests,
      {String? services}) async {
    final svcQuery = (services != null && services.isNotEmpty)
        ? '&services=${Uri.encodeQueryComponent(services)}'
        : '';
    final response = await ApiService.get(
        '/rooms/$roomId/calculate-price?checkIn=$checkIn&checkOut=$checkOut&guests=$guests$svcQuery');
    final data = ApiResponseParser.extractData(response);
    if (data is num) return data.toDouble();
    return double.parse(data.toString());
  }

  Future<Room?> getRoomById(int roomId) async {
    final response = await ApiService.get('/Rooms/$roomId');
    if (response.statusCode != 200) return null;
    try {
      return ApiResponseParser.parseObject(response, Room.fromJson);
    } catch (_) {
      return null;
    }
  }
}
