enum SupportTicketStatus { open, inProgress, closed }

enum SupportTicketPriority { low, medium, high }

SupportTicketStatus supportTicketStatusFromInt(int value) {
  switch (value) {
    case 1:
      return SupportTicketStatus.open;
    case 2:
      return SupportTicketStatus.inProgress;
    case 3:
      return SupportTicketStatus.closed;
    default:
      return SupportTicketStatus.open;
  }
}

int supportTicketStatusToInt(SupportTicketStatus status) {
  switch (status) {
    case SupportTicketStatus.open:
      return 1;
    case SupportTicketStatus.inProgress:
      return 2;
    case SupportTicketStatus.closed:
      return 3;
  }
}

SupportTicketPriority supportTicketPriorityFromInt(int value) {
  switch (value) {
    case 1:
      return SupportTicketPriority.low;
    case 2:
      return SupportTicketPriority.medium;
    case 3:
      return SupportTicketPriority.high;
    default:
      return SupportTicketPriority.medium;
  }
}

int supportTicketPriorityToInt(SupportTicketPriority priority) {
  switch (priority) {
    case SupportTicketPriority.low:
      return 1;
    case SupportTicketPriority.medium:
      return 2;
    case SupportTicketPriority.high:
      return 3;
  }
}

String supportTicketStatusLabel(SupportTicketStatus status) {
  switch (status) {
    case SupportTicketStatus.open:
      return 'Otvoren';
    case SupportTicketStatus.inProgress:
      return 'U toku';
    case SupportTicketStatus.closed:
      return 'Zatvoren';
  }
}

class SupportTicket {
  final int id;
  final int userId;
  final String userName;
  final String subject;
  final String messageBody;
  final SupportTicketStatus status;
  final SupportTicketPriority priority;
  final DateTime createdAt;
  final DateTime updatedAt;

  SupportTicket({
    required this.id,
    required this.userId,
    required this.userName,
    required this.subject,
    required this.messageBody,
    required this.status,
    required this.priority,
    required this.createdAt,
    required this.updatedAt,
  });

  factory SupportTicket.fromJson(Map<String, dynamic> json) => SupportTicket(
        id: json['id'] ?? 0,
        userId: json['userId'] ?? 0,
        userName: json['userName'] ?? '',
        subject: json['subject'] ?? '',
        messageBody: json['messageBody'] ?? '',
        status: supportTicketStatusFromInt(json['status'] ?? 1),
        priority: supportTicketPriorityFromInt(json['priority'] ?? 2),
        createdAt: DateTime.tryParse(json['createdAt'] ?? '') ?? DateTime.now(),
        updatedAt: DateTime.tryParse(json['updatedAt'] ?? '') ?? DateTime.now(),
      );
}
