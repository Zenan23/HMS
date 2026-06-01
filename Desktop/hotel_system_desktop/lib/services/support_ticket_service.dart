import '../models/support_ticket.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class SupportTicketService {
  final _api = ApiService();

  Future<PaginatedResult<SupportTicket>> getPaged(
      {int pageNumber = 1, int pageSize = 10}) async {
    final response = await _api.get(
        '/api/SupportTickets?pageNumber=$pageNumber&pageSize=$pageSize');
    return ApiResponseParser.parsePaginated(response, SupportTicket.fromJson);
  }

  Future<List<SupportTicket>> getByUserId(int userId) async {
    final response = await _api.get('/api/SupportTickets/user/$userId');
    return ApiResponseParser.parseList(response, SupportTicket.fromJson);
  }

  Future<List<SupportTicket>> getByStatus(SupportTicketStatus status) async {
    final response =
        await _api.get('/api/SupportTickets/status/${supportTicketStatusToInt(status)}');
    return ApiResponseParser.parseList(response, SupportTicket.fromJson);
  }

  Future<SupportTicket> create(Map<String, dynamic> body) async {
    final response = await _api.post('/api/SupportTickets', body);
    return ApiResponseParser.parseObject(response, SupportTicket.fromJson);
  }

  Future<SupportTicket> update(int id, Map<String, dynamic> body) async {
    final response = await _api.put('/api/SupportTickets/$id', body);
    return ApiResponseParser.parseObject(response, SupportTicket.fromJson);
  }

  Future<void> delete(int id) async {
    final response = await _api.delete('/api/SupportTickets/$id');
    ApiResponseParser.ensureSuccess(response);
  }
}
