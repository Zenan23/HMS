import '../services/api_service.dart';

String resolveImageUrl(String imageUrl) {
  if (imageUrl.startsWith('http://') || imageUrl.startsWith('https://')) {
    return imageUrl;
  }
  if (imageUrl.startsWith('/')) {
    return '${ApiService.baseUrl}$imageUrl';
  }
  return imageUrl;
}
