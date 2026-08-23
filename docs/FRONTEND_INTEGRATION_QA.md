# Frontend integracija – QA, rollout i metrike

## Test matrica po domeni

| Domena | CRUD | Filter | Paginacija | Auth/403 | Enum/Date parse |
|--------|------|--------|------------|----------|-----------------|
| SupportTickets | Desktop: da, Mobile: create+list | user/status | Desktop | JWT obavezno | status/priority |
| PriceAdjustments | Desktop: da, Mobile: read-only | active | Desktop | JWT obavezno | datumi |
| RoomMaintenanceLogs | Desktop: da | roomId | Desktop | JWT obavezno | datumi |
| InventoryTransactions | Desktop: da | item/staff | Desktop | JWT obavezno | quantity |
| LoyaltyPointsRedemptions | Desktop: da, Mobile: read-only | user/booking | Desktop | JWT obavezno | bodovi/vrijednost |

## Staged rollout

### Faza 1 – Desktop (operativa)
1. Deploy backend migracije (`AddOperationalTables`)
2. Verifikacija Desktop tabova: Podrška, Održavanje, Cijene
3. Smoke test CRUD za svaki modul sa Employee/Admin nalogom

### Faza 2 – Mobile (guest)
1. Support tiketi iz profila
2. Loyalty historija na profilu i rezervacijama
3. Price breakdown u booking flow-u

### Faza 3 – Proširenje
1. Inventory + Loyalty admin moduli (Desktop Employee)
2. Backend role-based ograničenja po modulu (opcionalno)

## Metrike usvajanja

- **SupportTickets**: broj otvorenih tiketa, prosječno vrijeme do zatvaranja
- **PriceAdjustments**: % booking flow-a sa prikazanim aktivnim pravilima
- **Loyalty**: broj redemption transakcija / korisnik / mjesec
- **Maintenance**: broj otvorenih kvarova po sobi, prosječni trošak popravke
- **Inventory**: broj transakcija po danu, negativne količine (potrošnja)

## Ručni test checklist

- [ ] Mobile login (Guest) + otvaranje Support ekrana
- [ ] Mobile kreiranje tiketa + lista tiketa
- [ ] Mobile booking sa prikazom aktivnih popusta
- [ ] Mobile profil prikazuje loyalty historiju
- [ ] Desktop Employee vidi operativne tabove
- [ ] Desktop Admin vidi Podršku i Cijene
- [ ] Desktop 403 prikazuje jasnu poruku (error_helper)
- [ ] CRUD ciklus za svaki novi Desktop modul
