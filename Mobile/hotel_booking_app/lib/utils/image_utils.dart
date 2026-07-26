import '../services/api_service.dart';

/// Pretvara [imageUrl] iz baze u puni URL za prikaz (upload slike sa Desktop app).
String resolveImageUrl(String? imageUrl) {
  if (imageUrl == null || imageUrl.isEmpty) return '';
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl;
  }
  final origin = ApiService.apiOrigin;
  if (imageUrl.startsWith('/')) {
    return '$origin$imageUrl';
  }
  return '$origin/$imageUrl';
}
