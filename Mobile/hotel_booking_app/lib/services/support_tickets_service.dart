import '../models/support_ticket.dart';
import '../utils/api_response.dart';
import 'api_service.dart';

class SupportTicketsService {
  Future<List<SupportTicket>> getByUserId(int userId) async {
    final response = await ApiService.get('/SupportTickets/user/$userId');
    return ApiResponseParser.parseList(response, SupportTicket.fromJson);
  }

  Future<SupportTicket> create({
    required int userId,
    required String subject,
    required String messageBody,
    SupportTicketPriority priority = SupportTicketPriority.medium,
  }) async {
    final response = await ApiService.post('/SupportTickets', {
      'userId': userId,
      'subject': subject,
      'messageBody': messageBody,
      'status': supportTicketStatusToInt(SupportTicketStatus.open),
      'priority': supportTicketPriorityToInt(priority),
    });
    return ApiResponseParser.parseObject(response, SupportTicket.fromJson);
  }

  Future<SupportTicket> getById(int id) async {
    final response = await ApiService.get('/SupportTickets/$id');
    return ApiResponseParser.parseObject(response, SupportTicket.fromJson);
  }
}
