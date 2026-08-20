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

  /// Rezervacije koje imaju bar jedan pokušaj plaćanja koji nije uspio
  /// (pending/failed/cancelled) — koristi se za "Plati ponovo" opciju.
  Future<List<Reservation>> fetchUnpaidReservations(int userId) async {
    final response = await ApiService.get('/Bookings/user/$userId/nopaid');
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

  /// Otkazivanje rezervacije. `reason` se šalje serveru (BookingsController -> CancelBooking)
  /// i upisuje u audit log; server sam provjerava vlasništvo preko JWT-a.
  Future<bool> cancelReservation(int id, {String? reason}) async {
    final response = await ApiService.post('/Bookings/$id/cancel', {
      if (reason != null && reason.trim().isNotEmpty) 'reason': reason.trim(),
    });
    ApiResponseParser.ensureSuccess(response);
    return true;
  }
}
