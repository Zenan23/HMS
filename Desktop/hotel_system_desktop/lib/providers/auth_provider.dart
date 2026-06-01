import 'package:flutter/material.dart';
import '../services/auth_service.dart';
import '../utils/role_utils.dart';

class AuthProvider with ChangeNotifier {
  bool isAuthenticated = false;
  bool isLoading = true;
  String? error;

  String? email;
  String? username;
  String? firstName;
  String? lastName;
  int? role; // čuvamo kao int

  bool get isAdmin => RoleUtils.isAdmin(role);
  bool get isEmployee => RoleUtils.isEmployee(role);

  AuthProvider() {
    _checkAuth();
  }

  Future<void> _checkAuth() async {
    isLoading = true;
    isAuthenticated = await AuthService().hasToken();
    if (isAuthenticated) {
      email = await AuthService().getEmail();
      username = await AuthService().getUsername();
      firstName = await AuthService().getFirstName();
      lastName = await AuthService().getLastName();
      role = await AuthService().getRoleInt();
    }
    isLoading = false;
    notifyListeners();
  }

  Future<void> login(String emailInput, String password) async {
    isLoading = true;
    notifyListeners();
    try {
      await AuthService().login(emailInput, password);
      isAuthenticated = true;
      error = null;
      email = await AuthService().getEmail();
      username = await AuthService().getUsername();
      firstName = await AuthService().getFirstName();
      lastName = await AuthService().getLastName();
      role = await AuthService().getRoleInt();
    } catch (e) {
      error = e.toString();
      isAuthenticated = false;
    }
    isLoading = false;
    notifyListeners();
  }

  Future<void> logout() async {
    await AuthService().logout();
    isAuthenticated = false;
    email = null;
    username = null;
    firstName = null;
    lastName = null;
    role = null;
    notifyListeners();
  }
}