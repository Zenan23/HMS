import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/room.dart';
import 'api_service.dart';

class RoomsService {
  Future<List<Room>> fetchRooms(
      {int page = 1, int pageSize = 10, String? filter}) async {
    final query =
        '?page=$page&pageSize=$pageSize${filter != null ? '&filter=$filter' : ''}';
    final response = await ApiService.get('/rooms$query');
    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((e) => Room.fromJson(e)).toList();
    } else {
      throw Exception('Greška pri dohvatu soba');
    }
  }

  Future<bool> checkAvailability(
      int roomId, String checkIn, String checkOut, {String? services}) async {
    final svcQuery = (services != null && services.isNotEmpty)
        ? '&services=${Uri.encodeQueryComponent(services)}'
        : '';
    final response = await ApiService.get(
        '/Rooms/$roomId/availability?checkIn=$checkIn&checkOut=$checkOut$svcQuery');
    if (response.statusCode == 200) {
      final decoded = jsonDecode(response.body);
      return decoded['data'] == true;
    } else {
      throw Exception('Greška pri provjeri dostupnosti.');
    }
  }

  Future<double> calculatePrice(
      int roomId, String checkIn, String checkOut, int guests,
      {String? services}) async {
    final svcQuery = (services != null && services.isNotEmpty)
        ? '&services=${Uri.encodeQueryComponent(services)}'
        : '';
    final response = await ApiService.get(
        '/rooms/$roomId/calculate-price?checkIn=$checkIn&checkOut=$checkOut&guests=$guests$svcQuery');
    if (response.statusCode == 200) {
      final decoded = jsonDecode(response.body);
      final price = (decoded['data'] is num)
          ? decoded['data'].toDouble()
          : double.parse(decoded['data'].toString());
      return price;
    } else {
      throw Exception('Greška pri izračunu cijene.');
    }
  }
}
