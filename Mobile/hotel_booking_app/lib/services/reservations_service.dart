import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/reservation.dart';
import 'api_service.dart';

class ReservationsService {
  Future<int> createBooking(Map<String, dynamic> data) async {
    final response = await ApiService.post('/Bookings', data);

    if (response.statusCode == 200 || response.statusCode == 201) {
      final resp = jsonDecode(response.body);
      if (resp is Map && resp['data'] != null) {
        final data = resp['data'];
        return data['id'] ?? data['bookingId'] ?? 0;
      }
      return resp['id'] ?? resp['bookingId'] ?? 0;
    } else {
      // Pokušaj parsirati response body za specifičnu poruku o grešci
      try {
        final errorData = jsonDecode(response.body) as Map<String, dynamic>;
        if (errorData.containsKey('message')) {
          throw Exception(errorData['message'] as String);
        }
      } catch (e) {
        // Ako ne možemo parsirati response body ili već imamo Exception, koristimo generičku poruku
        if (e is Exception) {
          throw e;
        }
      }
      throw Exception('Greška pri kreiranju rezervacije.');
    }
  }

  Future<List<Reservation>> fetchReservations() async {
    final response = await ApiService.get('/reservations');
    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((e) => Reservation.fromJson(e)).toList();
    } else {
      throw Exception('Greška pri dohvatu rezervacija');
    }
  }

  Future<List<Reservation>> fetchPaidReservations(int userId) async {
    final response = await ApiService.get('/Bookings/user/$userId/paid');
    if (response.statusCode == 200) {
      final resp = jsonDecode(response.body);
      final List data = resp['data'] ?? [];
      return data.map((e) => Reservation.fromJson(e)).toList();
    } else {
      throw Exception('Greška pri dohvatu plaćenih rezervacija');
    }
  }

  Future<bool> cancelReservation(int id) async {
    final response = await ApiService.post('/reservations/$id/cancel', {});
    return response.statusCode == 200;
  }

  Future<bool> refundReservation(int id) async {
    final response = await ApiService.post('/reservations/$id/refund', {});
    return response.statusCode == 200;
  }
}
/*
Korisnik otkazuje rezervaciju:
Prikaži dugme "Otkaži rezervaciju" na ekranu sa detaljima rezervacije.
Pošalji POST /api/bookings/{bookingId}/cancel sa svojim userId.
pozovi get /api/payments/booking/{bookingId} i dobij paymentId i spasi ga jer ce ti trebati za refund
Prikaži opciju "Zatraži povrat novca" ().
Pošalji POST /api/payments/{paymentId}/refund sa iznosom
*/
