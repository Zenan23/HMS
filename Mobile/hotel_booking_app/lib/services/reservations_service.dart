import '../models/reservation.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class ReservationsService {
  Future<int> createBooking(Map<String, dynamic> data) async {
    final response = await ApiService.post('/Bookings', data);
    final result = ApiResponseParser.parseObject(response, (json) => json);
    return result['id'] ?? result['bookingId'] ?? 0;
  }

  Future<List<Reservation>> fetchReservations() async {
    final response = await ApiService.get('/Bookings?pageNumber=1&pageSize=100');
    final result =
        ApiResponseParser.parsePaginated(response, Reservation.fromJson);
    return result.items;
  }

  Future<List<Reservation>> fetchPaidReservations(int userId) async {
    final response = await ApiService.get('/Bookings/user/$userId/paid');
    return ApiResponseParser.parseList(response, Reservation.fromJson);
  }

  Future<Reservation?> getReservationById(int id) async {
    final response = await ApiService.get('/Bookings/$id');
    if (response.statusCode != 200) return null;
    try {
      return ApiResponseParser.parseObject(response, Reservation.fromJson);
    } catch (_) {
      return null;
    }
  }

  Future<bool> cancelReservation(int id) async {
    final response = await ApiService.post('/Bookings/$id/cancel', {});
    ApiResponseParser.ensureSuccess(response);
    return true;
  }

  Future<bool> refundReservation(int id) async {
    final response = await ApiService.post('/Payments/booking/$id/refund', {});
    ApiResponseParser.ensureSuccess(response);
    return true;
  }
}
