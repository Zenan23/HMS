import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../services/api_service.dart';
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
    final hasToken = await AuthService().hasToken();
    if (!hasToken) {
      isAuthenticated = false;
      isLoading = false;
      notifyListeners();
      return;
    }

    // Validacija tokena — stari JWT nakon restarta API-ja inače daje 401 na sve.
    final tokenValid = await _validateToken();
    if (!tokenValid) {
      await AuthService().logout();
      isAuthenticated = false;
      isLoading = false;
      notifyListeners();
      return;
    }

    isAuthenticated = true;
    email = await AuthService().getEmail();
    username = await AuthService().getUsername();
    firstName = await AuthService().getFirstName();
    lastName = await AuthService().getLastName();
    role = await AuthService().getRoleInt();
    isLoading = false;
    notifyListeners();
  }

  Future<bool> _validateToken() async {
    try {
      final token = await AuthService().getToken();
      if (token == null || token.isEmpty) return false;
      final response = await http.get(
        Uri.parse('${ApiService.baseUrl}/api/auth/profile'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $token',
        },
      );
      return response.statusCode == 200;
    } catch (_) {
      return false;
    }
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
