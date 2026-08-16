import '../models/price_adjustment.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class PriceAdjustmentsService {
  Future<List<PriceAdjustment>> getActive({DateTime? atDate, int? hotelId}) async {
    final date = (atDate ?? DateTime.now()).toUtc().toIso8601String();
    final hotelParam = hotelId != null ? '&hotelId=$hotelId' : '';
    final response =
        await ApiService.get('/PriceAdjustments/active?atDate=$date$hotelParam');
    return ApiResponseParser.parseList(response, PriceAdjustment.fromJson);
  }

  double applyAdjustments(double basePrice, List<PriceAdjustment> adjustments) {
    if (adjustments.isEmpty) return basePrice;
    final cumulative = adjustments.where((a) => a.isCumulative).toList();
    final nonCumulative = adjustments.where((a) => !a.isCumulative).toList();

    double result = basePrice;
    for (final adj in cumulative) {
      result += result * (adj.percentageModifier / 100);
    }
    if (nonCumulative.isNotEmpty) {
      final best = nonCumulative.reduce((a, b) =>
          a.percentageModifier.abs() > b.percentageModifier.abs() ? a : b);
      result = basePrice + basePrice * (best.percentageModifier / 100);
    }
    return result < 0 ? 0 : result;
  }
}
