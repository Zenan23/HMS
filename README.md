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
pw: Leo123!

USER (gost)
email: marko@demo.com
pw: Marko123!

email: ana@demo.com
pw: Ana123!

email: ivan@demo.com
pw: Ivan123!


TEST PLAĆANJA

Stripe (test mode) — kartica:
broj: 4242 4242 4242 4242
datum isteka: bilo koji budući datum (npr. 12/34)
CVC: bilo koja 3 cifre (npr. 123)