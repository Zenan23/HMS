git clone <repository>
cd ebooking
docker-compose up --build -d
docker-compose up -d


LOGIN PODACI

Prijava se vrši putem email adrese (login endpoint zahtijeva validan email format).

ADMIN
email: admin@demo.com
pw: Admin123!

EMPLOYEE
email: leo@demo.com
pw: Leo1234!

USER (gost)
email: marko@demo.com
pw: Marko123!

email: ana@demo.com
pw: Ana1234!

email: ivan@demo.com
pw: Ivan123!


TEST PLAĆANJA

Sve tri metode idu kroz isti Stripe test nalog (Payment Sheet u mobilnoj app-i automatski nudi sve tri).

Kartica:
broj: 4242 4242 4242 4242
datum isteka: bilo koji budući datum (npr. 12/34)
CVC: bilo koja 3 cifre (npr. 123)

PayPal:
u Payment Sheet-u odabrati PayPal, dalje prati Stripe-ov test flow (bez potrebe za pravim PayPal nalogom).

Bankovni transfer (SEPA Direct Debit):
u Payment Sheet-u odabrati SEPA/bankovni transfer, unijeti test IBAN (npr. za Njemacku DE89370400440532013000 za trenutni uspjeh). Puna lista test IBAN-ova po zemlji: docs.stripe.com/testing
Napomena: SEPA potvrda zna potrajati par minuta (pogotovo sa "delayed" test IBAN-ovima) — to je normalno, ne greška. Ako se ne potvrdi odmah u app-u, rezervacija ostaje sačuvana kao "Plaćanje u obradi" u Moje rezervacije i potvrdiće se sama (webhook) ili ručno preko dugmeta "Provjeri status"

Testiranje zaboravljene lozinke i slanja koda se moze izvrsiti samo sa userima sa pravom email adresom!