import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../models/hotel.dart';
import '../models/room.dart';
import 'api_service.dart';

class HotelsService extends ChangeNotifier {
  List<Hotel> _hotels = [];
  List<Room> _hotelRooms = [];
  bool _isLoading = false;
  List<Hotel> _recommended = [];

  List<Hotel> get hotels => _hotels;
  List<Room> get hotelRooms => _hotelRooms;
  bool get isLoading => _isLoading;
  List<Hotel> get recommended => _recommended;

  set hotels(List<Hotel> hotels) {
    _hotels = hotels;
    notifyListeners();
  }

  Future<List<Hotel>> fetchHotels({int page = 1, int pageSize = 10}) async {
    final response =
        await ApiService.get('/Hotels?pageNumber=$page&pageSize=$pageSize');
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);

      final items = data['data']?['items'] ?? [];
      return (items as List).map((e) => Hotel.fromJson(e)).toList();
    } else {
      throw Exception('Greška pri dohvatu hotela');
    }
  }

  Future<List<Hotel>> fetchHotelsByCity(String city) async {
    final response = await ApiService.get('/Hotels/by-city/$city');
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      final items = data['data'] ?? [];
      return (items as List).map((e) => Hotel.fromJson(e)).toList();
    } else {
      throw Exception('Greška pri dohvatu hotela po gradu');
    }
  }

  Future<List<Hotel>> fetchHotelsByRating(int rating) async {
    final response = await ApiService.get('/Hotels/by-rating/$rating');
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      final items = data['data'] ?? [];
      return (items as List).map((e) => Hotel.fromJson(e)).toList();
    } else {
      throw Exception('Greška pri dohvatu hotela po ratingu');
    }
  }

  Future<void> fetchRecommended(int userId) async {
    try {
      final response = await ApiService.get('/Hotels/user/$userId/hotels');
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        final items = data['data'] ?? [];
        _recommended = (items as List).map((e) => Hotel.fromJson(e)).toList();
        notifyListeners();
      }
    } catch (_) {}
  }

  Future<String?> fetchHotelRooms(int hotelId, {String? token}) async {
    _isLoading = true;
    notifyListeners();

    try {
      final response = await ApiService.get('/Rooms/by-hotel/$hotelId');

      if (response.statusCode == 200) {
        final decoded = jsonDecode(response.body);

// "data" je zapravo lista soba
        final List<dynamic> roomsJson = decoded['data'] ?? [];
        _hotelRooms = roomsJson.map((json) => Room.fromJson(json)).toList();
        _isLoading = false;
        notifyListeners();
        return null;
      } else {
        _isLoading = false;
        notifyListeners();
        // Pokušaj parsirati response body za specifičnu poruku o grešci
        try {
          final errorData = jsonDecode(response.body) as Map<String, dynamic>;
          if (errorData.containsKey('message')) {
            return errorData['message'] as String;
          }
        } catch (e) {
          // Ako ne možemo parsirati response body, koristimo generičku poruku
        }
        return 'Greška pri dohvaćanju soba';
      }
    } catch (e) {
      _isLoading = false;
      notifyListeners();
      return 'Greška pri povezivanju sa serverom';
    }
  }

  Hotel? getHotelById(int id) {
    try {
      return _hotels.firstWhere((hotel) => hotel.id == id);
    } catch (e) {
      return null;
    }
  }

  void clearHotelRooms() {
    _hotelRooms = [];
    notifyListeners();
  }
}
