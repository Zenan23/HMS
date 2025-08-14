import 'dart:convert';
import 'api_service.dart';

class ReviewsService {
  Future<List<dynamic>> fetchReviews(int roomId) async {
    final response = await ApiService.get('/rooms/$roomId/reviews');
    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
      throw Exception('Greška pri dohvatu recenzija');
    }
  }

  Future<bool> addReview(int roomId, String text, int rating) async {
    final response = await ApiService.post('/rooms/$roomId/reviews', {
      'text': text,
      'rating': rating,
    });
    return response.statusCode == 201;
  }

  Future<bool> deleteReview(int reviewId) async {
    final response = await ApiService.post('/reviews/$reviewId/delete', {});
    return response.statusCode == 200;
  }
}