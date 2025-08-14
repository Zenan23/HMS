# Hotel Sistem Desktop (Flutter)

## Pokretanje
1. Instaliraj Flutter SDK (3.x)
2. Pokreni:
   ```
   flutter pub get
   flutter run -d windows
   ```

## Osnovne funkcionalnosti
- Prijava korisnika (JWT)
- Upravljački panel (CRUD): hoteli, sobe, usluge, korisnici, rezervacije
- Paginacija i filtriranje listi
- Preporuke (prikaz administrativno definisanih i/ili generisanih preporuka)
- Notifikacije (pregled sistemskih obavijesti po korisniku)
- Sigurno čuvanje tokena

## Struktura projekta
- `lib/models/` - modeli podataka
- `lib/services/` - API i auth servisi
- `lib/providers/` - Provider state management
- `lib/screens/` - Ekrani aplikacije
- `lib/widgets/` - Custom widgeti