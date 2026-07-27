import '../models/booking.dart';
import '../models/room.dart';
import '../models/support_ticket.dart';
import '../models/user.dart';

String userRoleLabel(UserRole role) {
  switch (role) {
    case UserRole.Guest:
      return 'Gost';
    case UserRole.Employee:
      return 'Uposlenik';
    case UserRole.Admin:
      return 'Administrator';
  }
}

String bookingStatusLabel(BookingStatus status) {
  switch (status) {
    case BookingStatus.Pending:
      return 'Na čekanju';
    case BookingStatus.Confirmed:
      return 'Potvrđeno';
    case BookingStatus.CheckedIn:
      return 'Prijavljen';
    case BookingStatus.CheckedOut:
      return 'Odjavljen';
    case BookingStatus.Cancelled:
      return 'Otkazano';
    case BookingStatus.NoShow:
      return 'Nije se pojavio';
  }
}

String roomTypeLabel(RoomType type) {
  switch (type) {
    case RoomType.Single:
      return 'Jednokrevetna';
    case RoomType.Double:
      return 'Dvokrevetna';
    case RoomType.Twin:
      return 'Twin';
    case RoomType.Suite:
      return 'Apartman';
    case RoomType.Deluxe:
      return 'Deluxe';
    case RoomType.Presidential:
      return 'Predsjednički';
  }
}

String supportTicketPriorityLabel(SupportTicketPriority priority) {
  switch (priority) {
    case SupportTicketPriority.low:
      return 'Nizak';
    case SupportTicketPriority.medium:
      return 'Srednji';
    case SupportTicketPriority.high:
      return 'Visok';
  }
}
