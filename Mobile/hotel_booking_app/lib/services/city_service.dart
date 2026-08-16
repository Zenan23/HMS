import '../models/city.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class CityService {
  /// Gradova obično ima malo — dohvati sve odjednom za dropdown/filter.
  Future<List<City>> fetchAll() async {
    final response = await ApiService.get('/Cities?pageNumber=1&pageSize=100');
    final result = ApiResponseParser.parsePaginated(response, City.fromJson);
    return result.items;
  }
}
