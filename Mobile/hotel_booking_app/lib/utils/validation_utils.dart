import 'package:flutter/material.dart';

class ValidationUtils {
  // Email validacija
  static String? validateEmail(String? value) {
    if (value == null || value.isEmpty) {
      return 'Email je obavezan';
    }
    
    // Regex za email validaciju
    final emailRegex = RegExp(r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$');
    if (!emailRegex.hasMatch(value)) {
      return 'Unesite validan email format (npr. korisnik@email.com)';
    }
    
    if (value.length > 100) {
      return 'Email ne može biti duži od 100 karaktera';
    }
    
    return null;
  }

  // Username validacija
  static String? validateUsername(String? value) {
    if (value == null || value.isEmpty) {
      return 'Korisničko ime je obavezno';
    }
    
    if (value.length < 3) {
      return 'Korisničko ime mora imati najmanje 3 karaktera';
    }
    
    if (value.length > 50) {
      return 'Korisničko ime ne može biti duže od 50 karaktera';
    }
    
    // Samo slova, brojevi i underscore
    final usernameRegex = RegExp(r'^[a-zA-Z0-9_]+$');
    if (!usernameRegex.hasMatch(value)) {
      return 'Korisničko ime može sadržati samo slova, brojeve i _';
    }
    
    return null;
  }

  // Password validacija
  static String? validatePassword(String? value) {
    if (value == null || value.isEmpty) {
      return 'Lozinka je obavezna';
    }
    
    if (value.length < 6) {
      return 'Lozinka mora imati najmanje 6 karaktera';
    }
    
    if (value.length > 100) {
      return 'Lozinka ne može biti duža od 100 karaktera';
    }
    
    return null;
  }

  // Ime validacija
  static String? validateFirstName(String? value) {
    if (value == null || value.isEmpty) {
      return 'Ime je obavezno';
    }
    
    if (value.length < 2) {
      return 'Ime mora imati najmanje 2 karaktera';
    }
    
    if (value.length > 50) {
      return 'Ime ne može biti duže od 50 karaktera';
    }
    
    // Samo slova i razmaci
    final nameRegex = RegExp(r'^[a-zA-ZčćđšžČĆĐŠŽ\s]+$');
    if (!nameRegex.hasMatch(value)) {
      return 'Ime može sadržati samo slova';
    }
    
    return null;
  }

  // Prezime validacija
  static String? validateLastName(String? value) {
    if (value == null || value.isEmpty) {
      return 'Prezime je obavezno';
    }
    
    if (value.length < 2) {
      return 'Prezime mora imati najmanje 2 karaktera';
    }
    
    if (value.length > 50) {
      return 'Prezime ne može biti duže od 50 karaktera';
    }
    
    // Samo slova i razmaci
    final nameRegex = RegExp(r'^[a-zA-ZčćđšžČĆĐŠŽ\s]+$');
    if (!nameRegex.hasMatch(value)) {
      return 'Prezime može sadržati samo slova';
    }
    
    return null;
  }

  // Telefon validacija
  static String? validatePhoneNumber(String? value) {
    if (value == null || value.isEmpty) {
      return 'Broj telefona je obavezan';
    }
    
    // Ukloni sve razmake i specijalne karaktere
    final cleanNumber = value.replaceAll(RegExp(r'[\s\-\(\)\+]'), '');
    
    if (cleanNumber.length < 8) {
      return 'Broj telefona mora imati najmanje 8 cifara';
    }
    
    if (cleanNumber.length > 15) {
      return 'Broj telefona ne može biti duži od 15 cifara';
    }
    
    // Samo brojevi
    final phoneRegex = RegExp(r'^[0-9]+$');
    if (!phoneRegex.hasMatch(cleanNumber)) {
      return 'Broj telefona može sadržati samo brojeve';
    }
    
    return null;
  }

  // Broj gostiju validacija
  static String? validateGuestCount(String? value) {
    if (value == null || value.isEmpty) {
      return 'Broj gostiju je obavezan';
    }
    
    final guestCount = int.tryParse(value);
    if (guestCount == null) {
      return 'Unesite validan broj gostiju';
    }
    
    if (guestCount <= 0) {
      return 'Broj gostiju mora biti veći od 0';
    }
    
    if (guestCount > 10) {
      return 'Broj gostiju ne može biti veći od 10';
    }
    
    return null;
  }

  // Datum validacija
  static String? validateDate(String? value) {
    if (value == null || value.isEmpty) {
      return 'Datum je obavezan';
    }
    
    final date = DateTime.tryParse(value);
    if (date == null) {
      return 'Unesite validan datum';
    }
    
    if (date.isBefore(DateTime.now().subtract(const Duration(days: 1)))) {
      return 'Datum ne može biti u prošlosti';
    }
    
    return null;
  }

  // Ocjena validacija
  static String? validateRating(String? value) {
    if (value == null || value.isEmpty) {
      return 'Ocjena je obavezna';
    }
    
    final rating = double.tryParse(value);
    if (rating == null) {
      return 'Unesite validan broj za ocjenu';
    }
    
    if (rating < 1 || rating > 5) {
      return 'Ocjena mora biti između 1 i 5';
    }
    
    return null;
  }

  // Komentar validacija
  static String? validateComment(String? value) {
    if (value == null || value.isEmpty) {
      return 'Komentar je obavezan';
    }
    
    if (value.length < 10) {
      return 'Komentar mora imati najmanje 10 karaktera';
    }
    
    if (value.length > 500) {
      return 'Komentar ne može biti duži od 500 karaktera';
    }
    
    return null;
  }

  // Broj kartice validacija
  static String? validateCardNumber(String? value) {
    if (value == null || value.isEmpty) {
      return 'Broj kartice je obavezan';
    }
    
    // Ukloni razmake
    final cleanNumber = value.replaceAll(RegExp(r'\s'), '');
    
    if (cleanNumber.length < 13 || cleanNumber.length > 19) {
      return 'Broj kartice mora imati 13-19 cifara';
    }
    
    // Samo brojevi
    final cardRegex = RegExp(r'^[0-9]+$');
    if (!cardRegex.hasMatch(cleanNumber)) {
      return 'Broj kartice može sadržati samo brojeve';
    }
    
    return null;
  }

  // CVV validacija
  static String? validateCVV(String? value) {
    if (value == null || value.isEmpty) {
      return 'CVV je obavezan';
    }
    
    if (value.length < 3 || value.length > 4) {
      return 'CVV mora imati 3-4 cifre';
    }
    
    // Samo brojevi
    final cvvRegex = RegExp(r'^[0-9]+$');
    if (!cvvRegex.hasMatch(value)) {
      return 'CVV može sadržati samo brojeve';
    }
    
    return null;
  }

  // Datum isteka kartice validacija
  static String? validateExpiryDate(String? value) {
    if (value == null || value.isEmpty) {
      return 'Datum isteka je obavezan';
    }
    
    // Format MM/YY
    final expiryRegex = RegExp(r'^([0-9]{2})/([0-9]{2})$');
    final match = expiryRegex.firstMatch(value);
    
    if (match == null) {
      return 'Format: MM/YY (npr. 12/25)';
    }
    
    final month = int.tryParse(match.group(1)!);
    final year = int.tryParse(match.group(2)!);
    
    if (month == null || year == null) {
      return 'Unesite validan datum';
    }
    
    if (month < 1 || month > 12) {
      return 'Mjesec mora biti između 01 i 12';
    }
    
    final now = DateTime.now();
    final currentYear = now.year % 100; // Zadnje 2 cifre godine
    final currentMonth = now.month;
    
    if (year < currentYear || (year == currentYear && month < currentMonth)) {
      return 'Kartica je istekla';
    }
    
    return null;
  }

  // Ime na kartici validacija
  static String? validateCardholderName(String? value) {
    if (value == null || value.isEmpty) {
      return 'Ime na kartici je obavezno';
    }
    
    if (value.length < 2) {
      return 'Ime na kartici mora imati najmanje 2 karaktera';
    }
    
    if (value.length > 50) {
      return 'Ime na kartici ne može biti duže od 50 karaktera';
    }
    
    // Samo slova i razmaci
    final nameRegex = RegExp(r'^[a-zA-ZčćđšžČĆĐŠŽ\s]+$');
    if (!nameRegex.hasMatch(value)) {
      return 'Ime na kartici može sadržati samo slova';
    }
    
    return null;
  }

  // Adresa validacija
  static String? validateAddress(String? value) {
    if (value == null || value.isEmpty) {
      return 'Adresa je obavezna';
    }
    
    if (value.length < 10) {
      return 'Adresa mora imati najmanje 10 karaktera';
    }
    
    if (value.length > 200) {
      return 'Adresa ne može biti duža od 200 karaktera';
    }
    
    return null;
  }

  // Grad validacija
  static String? validateCity(String? value) {
    if (value == null || value.isEmpty) {
      return 'Grad je obavezan';
    }
    
    if (value.length < 2) {
      return 'Grad mora imati najmanje 2 karaktera';
    }
    
    if (value.length > 100) {
      return 'Grad ne može biti duži od 100 karaktera';
    }
    
    // Samo slova i razmaci
    final cityRegex = RegExp(r'^[a-zA-ZčćđšžČĆĐŠŽ\s]+$');
    if (!cityRegex.hasMatch(value)) {
      return 'Grad može sadržati samo slova';
    }
    
    return null;
  }

  // Poštanski broj validacija
  static String? validatePostalCode(String? value) {
    if (value == null || value.isEmpty) {
      return 'Poštanski broj je obavezan';
    }
    
    if (value.length < 4 || value.length > 10) {
      return 'Poštanski broj mora imati 4-10 karaktera';
    }
    
    // Slova, brojevi i razmaci
    final postalRegex = RegExp(r'^[a-zA-Z0-9\s]+$');
    if (!postalRegex.hasMatch(value)) {
      return 'Poštanski broj može sadržati samo slova, brojeve i razmake';
    }
    
    return null;
  }

  // Država validacija
  static String? validateCountry(String? value) {
    if (value == null || value.isEmpty) {
      return 'Država je obavezna';
    }
    
    if (value.length < 2) {
      return 'Država mora imati najmanje 2 karaktera';
    }
    
    if (value.length > 100) {
      return 'Država ne može biti duža od 100 karaktera';
    }
    
    // Samo slova i razmaci
    final countryRegex = RegExp(r'^[a-zA-ZčćđšžČĆĐŠŽ\s]+$');
    if (!countryRegex.hasMatch(value)) {
      return 'Država može sadržati samo slova';
    }
    
    return null;
  }
}
