import '../models/user.dart';

class RoleUtils {
  static bool isAdmin(int? role) => role == UserRole.Admin.index;
  static bool isEmployee(int? role) => role == UserRole.Employee.index;
  static bool isGuest(int? role) => role == UserRole.Guest.index;

  static bool canAccessOperationalModules(int? role) =>
      isAdmin(role) || isEmployee(role);

  static bool canAccessInventory(int? role) =>
      isAdmin(role) || isEmployee(role);

  static bool canAccessSupport(int? role) =>
      isAdmin(role) || isEmployee(role) || isGuest(role);
}
