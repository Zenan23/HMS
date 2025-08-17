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

  // Naziv hotela validacija
  static String? validateHotelName(String? value) {
    if (value == null || value.isEmpty) {
      return 'Naziv hotela je obavezan';
    }
    
    if (value.length < 3) {
      return 'Naziv hotela mora imati najmanje 3 karaktera';
    }
    
    if (value.length > 200) {
      return 'Naziv hotela ne može biti duži od 200 karaktera';
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
    
    if (value.length > 500) {
      return 'Adresa ne može biti duža od 500 karaktera';
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

  // Opis validacija
  static String? validateDescription(String? value) {
    if (value == null || value.isEmpty) {
      return 'Opis je obavezan';
    }
    
    if (value.length < 10) {
      return 'Opis mora imati najmanje 10 karaktera';
    }
    
    if (value.length > 1000) {
      return 'Opis ne može biti duži od 1000 karaktera';
    }
    
    return null;
  }

  // Cijena validacija
  static String? validatePrice(String? value) {
    if (value == null || value.isEmpty) {
      return 'Cijena je obavezna';
    }
    
    final price = double.tryParse(value);
    if (price == null) {
      return 'Unesite validan broj za cijenu';
    }
    
    if (price <= 0) {
      return 'Cijena mora biti veća od 0';
    }
    
    if (price > 10000) {
      return 'Cijena ne može biti veća od 10,000';
    }
    
    return null;
  }

  // Broj sobe validacija
  static String? validateRoomNumber(String? value) {
    if (value == null || value.isEmpty) {
      return 'Broj sobe je obavezan';
    }
    
    final roomNumber = int.tryParse(value);
    if (roomNumber == null) {
      return 'Unesite validan broj sobe';
    }
    
    if (roomNumber <= 0) {
      return 'Broj sobe mora biti veći od 0';
    }
    
    if (roomNumber > 9999) {
      return 'Broj sobe ne može biti veći od 9999';
    }
    
    return null;
  }

  // Kapacitet sobe validacija
  static String? validateRoomCapacity(String? value) {
    if (value == null || value.isEmpty) {
      return 'Kapacitet je obavezan';
    }
    
    final capacity = int.tryParse(value);
    if (capacity == null) {
      return 'Unesite validan broj za kapacitet';
    }
    
    if (capacity <= 0) {
      return 'Kapacitet mora biti veći od 0';
    }
    
    if (capacity > 10) {
      return 'Kapacitet ne može biti veći od 10';
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
}
