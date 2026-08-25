using Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Contracts.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Redoslijed odgovara seed hotelima (1-based index u imenu fajla).
        /// Fajlovi žive u API/SeedAssets/hotels/ i kopiraju se u uploads/hotels/ preko IFileStorageService.
        /// </summary>
        private static readonly string[] HotelSeedImageFiles =
        {
            "hotel_1.jpg",
            "hotel_2.jpg",
            "hotel_3.jpg",
            "hotel_4.jpg",
            "hotel_5.jpg",
        };

        public static async Task InitializeAsync(ApplicationDbContext context, IServiceProvider services)
        {
            // Referentne/šifarnik tabele: države i gradovi. Moraju postojati PRIJE hotela
            // jer Hotel.CityId je obavezan FK (zamjena za slobodan tekstualni unos grada/države).
            if (!await context.Countries.AnyAsync())
            {
                context.Countries.AddRange(
                    new Country { Name = "Hrvatska" },
                    new Country { Name = "Slovenija" },
                    new Country { Name = "Bosna i Hercegovina" }
                );
                await context.SaveChangesAsync();
            }

            // Referentna/šifarnik tabela kategorija servisa. Mora postojati PRIJE servisa
            // jer Service.ServiceCategoryId je obavezan FK (zamjena za slobodan tekstualni unos).
            if (!await context.ServiceCategories.AnyAsync())
            {
                context.ServiceCategories.AddRange(
                    new ServiceCategory { Name = "Spa" },
                    new ServiceCategory { Name = "Food" },
                    new ServiceCategory { Name = "Transport" }
                );
                await context.SaveChangesAsync();
            }

            var hrCountry = await context.Countries.FirstAsync(c => c.Name == "Hrvatska");
            var siCountry = await context.Countries.FirstAsync(c => c.Name == "Slovenija");
            var baCountry = await context.Countries.FirstAsync(c => c.Name == "Bosna i Hercegovina");

            if (!await context.Cities.AnyAsync())
            {
                context.Cities.AddRange(
                    new City { Name = "Split", CountryId = hrCountry.Id },
                    new City { Name = "Zagreb", CountryId = hrCountry.Id },
                    new City { Name = "Bled", CountryId = siCountry.Id },
                    new City { Name = "Sarajevo", CountryId = baCountry.Id },
                    new City { Name = "Mostar", CountryId = baCountry.Id }
                );
                await context.SaveChangesAsync();
            }

            // Hoteli + sobe + servisi (osnovni seed)
            if (!await context.Hotels.AnyAsync())
            {
                var splitCity = await context.Cities.FirstAsync(c => c.Name == "Split");
                var bledCity = await context.Cities.FirstAsync(c => c.Name == "Bled");
                var sarajevoCity = await context.Cities.FirstAsync(c => c.Name == "Sarajevo");
                var mostarCity = await context.Cities.FirstAsync(c => c.Name == "Mostar");
                var zagrebCity = await context.Cities.FirstAsync(c => c.Name == "Zagreb");

                var hotels = new List<Hotel>
                {
                    new Hotel { Name = "Blue Sea Hotel", Address = "Riviera 1", CityId = splitCity.Id, PhoneNumber = "+385 21 123 456", Email = "info@bluesea.hr", Description = "Hotel uz more sa predivnim pogledom", ImageUrl = string.Empty },
                    new Hotel { Name = "Alpine Lodge", Address = "Dolomiti 12", CityId = bledCity.Id, PhoneNumber = "+386 4 987 654", Email = "info@alpinelodge.si", Description = "Planinski ugođaj i wellness", ImageUrl = string.Empty },
                    new Hotel { Name = "City Center Inn", Address = "King St 10", CityId = sarajevoCity.Id, PhoneNumber = "+387 33 111 222", Email = "info@citycenter.ba", Description = "U srcu grada, blizu svih atrakcija", ImageUrl = string.Empty },
                    new Hotel { Name = "Riverside Retreat", Address = "Obala 5", CityId = mostarCity.Id, PhoneNumber = "+387 36 555 777", Email = "hello@riverside.ba", Description = "Ugodan boravak uz rijeku", ImageUrl = string.Empty },
                    new Hotel { Name = "Metropolis Hotel", Address = "Main Ave 44", CityId = zagrebCity.Id, PhoneNumber = "+385 1 222 333", Email = "contact@metropolis.hr", Description = "Moderan gradski hotel", ImageUrl = string.Empty },
                };
                context.Hotels.AddRange(hotels);
                await context.SaveChangesAsync();

                // Slike: isti mehanizam kao upload endpoint (uploads/hotels + relativni ImageUrl)
                await AssignSeedHotelImagesAsync(context, services, hotels);

                // Sobe (više po hotelu)
                var rooms = new List<Room>();
                foreach (var h in hotels)
                {
                    rooms.AddRange(new[]
                    {
                        new Room { HotelId = h.Id, RoomNumber = $"{h.Id}01", RoomType = RoomType.Suite, PricePerNight = 80 + h.Id * 5, MaxOccupancy = 2, Description = "Komforna soba", IsAvailable = true },
                        new Room { HotelId = h.Id, RoomNumber = $"{h.Id}02", RoomType = RoomType.Deluxe, PricePerNight = 120 + h.Id * 5, MaxOccupancy = 3, Description = "Deluxe soba", IsAvailable = true },
                        new Room { HotelId = h.Id, RoomNumber = $"{h.Id}03", RoomType = RoomType.Presidential, PricePerNight = 160 + h.Id * 5, MaxOccupancy = 4, Description = "Veliki suite", IsAvailable = true }
                    });
                }
                context.Rooms.AddRange(rooms);
                await context.SaveChangesAsync();

                // Servisi po hotelu
                var spaCategory = await context.ServiceCategories.FirstAsync(sc => sc.Name == "Spa");
                var foodCategory = await context.ServiceCategories.FirstAsync(sc => sc.Name == "Food");
                var transportCategory = await context.ServiceCategories.FirstAsync(sc => sc.Name == "Transport");

                var servicesList = new List<Service>();
                foreach (var h in hotels)
                {
                    servicesList.AddRange(new[]
                    {
                        new Service { HotelId = h.Id, Name = "Spa paket", Description = "Sauna i masaža 60min", ServiceCategoryId = spaCategory.Id, Price = 30, IsAvailable = true, IsActive = true },
                        new Service { HotelId = h.Id, Name = "Doručak", Description = "Buffet doručak", ServiceCategoryId = foodCategory.Id, Price = 8, IsAvailable = true, IsActive = true },
                        new Service { HotelId = h.Id, Name = "Aerodrom shuttle", Description = "Prevoz do aerodroma", ServiceCategoryId = transportCategory.Id, Price = 25, IsAvailable = true, IsActive = true },
                    });
                }
                context.Services.AddRange(servicesList);
                await context.SaveChangesAsync();
            }

            // Users + primjeri rezervacija i recenzija (za preporuke)
            if (!await context.Users.AnyAsync())
            {
                var passwordService = services.GetService<IPasswordService>();
                var admin = new User { Username = "admin", Email = "admin@demo.com", FirstName = "Admin", LastName = "User", PhoneNumber = "+387 61 100 100", Role = UserRole.Admin, IsActive = true };
                var demo = new User { Username = "demo", Email = "demo@demo.com", FirstName = "Demo", LastName = "User", PhoneNumber = "+387 61 200 200", Role = UserRole.Guest, IsActive = true };
                var ana = new User { Username = "ana", Email = "ana@demo.com", FirstName = "Ana", LastName = "Anić", PhoneNumber = "+385 91 234 567", Role = UserRole.Guest, IsActive = true };
                var marko = new User { Username = "marko", Email = "marko@demo.com", FirstName = "Marko", LastName = "Marković", PhoneNumber = "+385 98 345 678", Role = UserRole.Guest, IsActive = true };
                var ivan = new User { Username = "ivan", Email = "ivan@demo.com", FirstName = "Ivan", LastName = "Ivić", PhoneNumber = "+387 62 456 789", Role = UserRole.Guest, IsActive = true };
                var leo = new User { Username = "leo", Email = "leo@demo.com", FirstName = "Leo", LastName = "Leić", PhoneNumber = "+387 63 567 890", Role = UserRole.Employee, IsActive = true };

                // IPasswordService mora biti registrovan (API/Worker Program.cs) — bez njega bi seed
                // korisnici dobili prazan PasswordHash (validno za NOT NULL kolonu, ali se ne mogu
                // prijaviti, bez ikakve greške). Radije odmah prekinuti seed nego tiho napraviti
                // naloge koje niko ne može koristiti.
                if (passwordService == null)
                {
                    throw new InvalidOperationException(
                        "SeedData: IPasswordService nije registrovan u DI kontejneru — ne mogu seed-ovati korisnike sa validnim PasswordHash-om.");
                }

                // Sve seed lozinke moraju zadovoljiti isto pravilo kao registracija (8+ karaktera,
                // veliko i malo slovo, broj, specijalni karakter — vidi RegisterDto.Password) — Ana i
                // Leo su prekratke sa samo 7 karaktera, ostale su već zadovoljavale i prije ovog.
                admin.PasswordHash = passwordService.HashPassword("Admin123!");
                demo.PasswordHash = passwordService.HashPassword("Demo123!");
                ana.PasswordHash = passwordService.HashPassword("Ana1234!");
                marko.PasswordHash = passwordService.HashPassword("Marko123!");
                ivan.PasswordHash = passwordService.HashPassword("Ivan123!");
                leo.PasswordHash = passwordService.HashPassword("Leo1234!");
                context.Users.AddRange(admin, demo, ana, marko, ivan, leo);
                await context.SaveChangesAsync();

                var hotelsAll = await context.Hotels.Include(h => h.City).ToListAsync();
                var roomsAll = await context.Rooms.ToListAsync();
                var servicesAll = await context.Services.Include(s => s.ServiceCategory).ToListAsync();

                // NAMJERNO ne vraća prvu sobu hotela (npr. "101", "201"...) — to je soba koju bi
                // testirao/kliknuo bilo ko prvi (npr. profesor na odbrani), pa ne treba da već ima
                // seed rezervaciju na sebi (može djelovati zauzeto/čudno kad se testira "od nule").
                // Umjesto toga vraća DRUGU sobu hotela ("02"), sa fallback-om na prvu ako hotel iz
                // nekog razloga ima samo jednu sobu.
                Room? RoomForHotel(int hotelId) =>
                    roomsAll.Where(r => r.HotelId == hotelId).OrderBy(r => r.Id).Skip(1).FirstOrDefault()
                    ?? roomsAll.FirstOrDefault(r => r.HotelId == hotelId);
                Service? ServiceForHotel(int hotelId, string category) => servicesAll.FirstOrDefault(s => s.HotelId == hotelId && s.ServiceCategory != null && s.ServiceCategory.Name == category);

                var today = DateTime.UtcNow.Date;
                var bookings = new List<Booking>();

                // Demo voli more (Split), često uzima spa i doručak
                var splitHotel = hotelsAll.FirstOrDefault(h => h.City != null && h.City.Name == "Split");
                if (splitHotel != null)
                {
                    var room = RoomForHotel(splitHotel.Id)!;
                    bookings.Add(new Booking
                    {
                        RoomId = room.Id,
                        UserId = demo.Id,
                        CheckInDate = today.AddDays(-20),
                        CheckOutDate = today.AddDays(-17),
                        NumberOfGuests = 2,
                        Status = BookingStatus.CheckedOut,
                        SpecialRequests = "Kasni check-in",
                        TotalPrice = 3 * room.PricePerNight
                    });
                }

                // Ana preferira planine (Bled)
                var bledHotel = hotelsAll.FirstOrDefault(h => h.City != null && h.City.Name == "Bled");
                if (bledHotel != null)
                {
                    var room = RoomForHotel(bledHotel.Id)!;
                    bookings.Add(new Booking
                    {
                        RoomId = room.Id,
                        UserId = ana.Id,
                        CheckInDate = today.AddDays(-10),
                        CheckOutDate = today.AddDays(-8),
                        NumberOfGuests = 2,
                        Status = BookingStatus.CheckedOut,
                        SpecialRequests = "Tiha soba",
                        TotalPrice = 2 * room.PricePerNight
                    });
                }

                // Marko često u gradskim hotelima (Zagreb/Sarajevo)
                var zagrebHotel = hotelsAll.FirstOrDefault(h => h.City != null && h.City.Name == "Zagreb");
                if (zagrebHotel != null)
                {
                    var room = RoomForHotel(zagrebHotel.Id)!;
                    bookings.Add(new Booking
                    {
                        RoomId = room.Id,
                        UserId = marko.Id,
                        CheckInDate = today.AddDays(-5),
                        CheckOutDate = today.AddDays(-3),
                        NumberOfGuests = 1,
                        Status = BookingStatus.CheckedOut,
                        SpecialRequests = "Radni sto",
                        TotalPrice = 2 * room.PricePerNight
                    });
                }

                var sarajevoHotel = hotelsAll.FirstOrDefault(h => h.City != null && h.City.Name == "Sarajevo");
                if (sarajevoHotel != null)
                {
                    var room = RoomForHotel(sarajevoHotel.Id)!;
                    bookings.Add(new Booking
                    {
                        RoomId = room.Id,
                        UserId = marko.Id,
                        CheckInDate = today.AddDays(5),
                        CheckOutDate = today.AddDays(7),
                        NumberOfGuests = 1,
                        Status = BookingStatus.Confirmed,
                        SpecialRequests = "Kasni check-out",
                        TotalPrice = 2 * room.PricePerNight
                    });
                }

                // Ivan voli Mostar i uslugu shuttle-a
                var mostarHotel = hotelsAll.FirstOrDefault(h => h.City != null && h.City.Name == "Mostar");
                if (mostarHotel != null)
                {
                    var room = RoomForHotel(mostarHotel.Id)!;
                    bookings.Add(new Booking
                    {
                        RoomId = room.Id,
                        UserId = ivan.Id,
                        CheckInDate = today.AddDays(-2),
                        CheckOutDate = today.AddDays(1),
                        NumberOfGuests = 2,
                        Status = BookingStatus.Confirmed,
                        SpecialRequests = "Pogled na rijeku",
                        TotalPrice = 3 * room.PricePerNight
                    });
                }

                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();

                // BookingServices za dio rezervacija (spa, doručak, shuttle)
                foreach (var b in bookings)
                {
                    var hotelId = roomsAll.First(r => r.Id == b.RoomId).HotelId;
                    var spa = ServiceForHotel(hotelId, "Spa");
                    var food = ServiceForHotel(hotelId, "Food");
                    var shuttle = ServiceForHotel(hotelId, "Transport");

                    var bs = new List<BookingService>();
                    if (spa != null) bs.Add(new BookingService { BookingId = b.Id, ServiceId = spa.Id, Quantity = 1, UnitPrice = spa.Price });
                    if (food != null) bs.Add(new BookingService { BookingId = b.Id, ServiceId = food.Id, Quantity = 2, UnitPrice = food.Price });
                    if (shuttle != null && b.UserId == ivan.Id) bs.Add(new BookingService { BookingId = b.Id, ServiceId = shuttle.Id, Quantity = 1, UnitPrice = shuttle.Price });
                    if (bs.Count > 0)
                    {
                        context.BookingServices.AddRange(bs);
                    }
                }
                await context.SaveChangesAsync();

                // Recenzije raznih korisnika (za collaborative filtering)
                var reviews = new List<Review>();
                
                // Demo - voli more i luksuz (visoke ocjene)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Split").Id, UserId = demo.Id, Rating = 5, Title = "Odlično more", Comment = "Prekrasan pogled na more", ReviewDate = today.AddDays(-15), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Zagreb").Id, UserId = demo.Id, Rating = 4, Title = "Dobar gradski hotel", Comment = "Moderan i udoban", ReviewDate = today.AddDays(-12), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Bled").Id, UserId = demo.Id, Rating = 3, Title = "Planinski ugođaj", Comment = "Nije moj stil", ReviewDate = today.AddDays(-10), IsApproved = true });
                
                // Ana - voli planine i prirodu (srednje ocjene)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Bled").Id, UserId = ana.Id, Rating = 5, Title = "Prekrasne planine", Comment = "Odličan wellness", ReviewDate = today.AddDays(-9), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Mostar").Id, UserId = ana.Id, Rating = 4, Title = "Lijep grad", Comment = "Ugodan boravak", ReviewDate = today.AddDays(-7), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Split").Id, UserId = ana.Id, Rating = 2, Title = "Previše turista", Comment = "Gradski hotel", ReviewDate = today.AddDays(-5), IsApproved = true });
                
                // Ivan - voli historijske gradove (visoke ocjene za historijske gradove)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Mostar").Id, UserId = ivan.Id, Rating = 5, Title = "Historijski grad", Comment = "Prelijep most", ReviewDate = today.AddDays(-3), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Sarajevo").Id, UserId = ivan.Id, Rating = 4, Title = "Baščaršija", Comment = "Odlična kultura", ReviewDate = today.AddDays(-1), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Zagreb").Id, UserId = ivan.Id, Rating = 3, Title = "Moderan grad", Comment = "Ok, ali nije historijski", ReviewDate = today.AddDays(-8), IsApproved = true });
                
                // Marko - voli luksuz i visoke ocjene (sličan Demo-u)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Split").Id, UserId = marko.Id, Rating = 5, Title = "Luksuzan boravak", Comment = "Odličan spa", ReviewDate = today.AddDays(-6), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Zagreb").Id, UserId = marko.Id, Rating = 4, Title = "Moderan luksuz", Comment = "Dobar standard", ReviewDate = today.AddDays(-4), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City != null && h.City.Name == "Bled").Id, UserId = marko.Id, Rating = 4, Title = "Planinski luksuz", Comment = "Odličan wellness", ReviewDate = today.AddDays(-2), IsApproved = true });
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }

            // Za već postojeće baze sa starim vanjskim URL-ovima — zamijeni lokalnim upload putanjama
            await MigrateExternalHotelImagesAsync(context, services);

            await SeedOperationalDataAsync(context);
        }

        private static async Task AssignSeedHotelImagesAsync(
            ApplicationDbContext context,
            IServiceProvider services,
            IReadOnlyList<Hotel> hotels)
        {
            var fileStorage = services.GetService<IFileStorageService>();
            if (fileStorage == null)
                return;

            var seedDir = ResolveSeedHotelsDirectory();
            if (seedDir == null)
                return;

            for (var i = 0; i < hotels.Count && i < HotelSeedImageFiles.Length; i++)
            {
                var fileName = HotelSeedImageFiles[i];
                var sourcePath = Path.Combine(seedDir, fileName);
                if (!File.Exists(sourcePath))
                    continue;

                await using var stream = File.OpenRead(sourcePath);
                hotels[i].ImageUrl = await fileStorage.SaveHotelImageAsync(hotels[i].Id, stream, fileName);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Jednokratno: hoteli čiji ImageUrl nije managed /uploads/... putanja dobiju seed fajl.
        /// </summary>
        private static async Task MigrateExternalHotelImagesAsync(ApplicationDbContext context, IServiceProvider services)
        {
            var fileStorage = services.GetService<IFileStorageService>();
            if (fileStorage == null)
                return;

            var hotels = await context.Hotels.OrderBy(h => h.Id).ToListAsync();
            var needsMigration = hotels
                .Where(h => !fileStorage.IsManagedPath(h.ImageUrl))
                .ToList();
            if (needsMigration.Count == 0)
                return;

            var seedDir = ResolveSeedHotelsDirectory();
            if (seedDir == null)
                return;

            var ordered = hotels.ToList();
            foreach (var hotel in needsMigration)
            {
                var index = ordered.FindIndex(h => h.Id == hotel.Id);
                if (index < 0 || index >= HotelSeedImageFiles.Length)
                    continue;

                var fileName = HotelSeedImageFiles[index];
                var sourcePath = Path.Combine(seedDir, fileName);
                if (!File.Exists(sourcePath))
                    continue;

                await using var stream = File.OpenRead(sourcePath);
                hotel.ImageUrl = await fileStorage.SaveHotelImageAsync(hotel.Id, stream, fileName);
            }

            await context.SaveChangesAsync();
        }

        private static string? ResolveSeedHotelsDirectory()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "SeedAssets", "hotels"),
                Path.Combine(Directory.GetCurrentDirectory(), "SeedAssets", "hotels"),
            };

            return candidates.FirstOrDefault(Directory.Exists);
        }

        private static async Task SeedOperationalDataAsync(ApplicationDbContext context)
        {
            var today = DateTime.UtcNow.Date;

            var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@demo.com");
            var firstHotelForPricing = await context.Hotels.OrderBy(h => h.Id).FirstOrDefaultAsync();

            if (!await context.PriceAdjustments.AnyAsync())
            {
                context.PriceAdjustments.AddRange(
                    new PriceAdjustment
                    {
                        Name = "Summer Sale",
                        PercentageModifier = -15m,
                        StartDate = today.AddDays(-30),
                        EndDate = today.AddDays(90),
                        IsCumulative = true,
                        CreatedByUserId = admin?.Id,
                        HotelId = null // sajt-wide
                    },
                    new PriceAdjustment
                    {
                        Name = "Early Bird",
                        PercentageModifier = -10m,
                        StartDate = today,
                        EndDate = today.AddDays(60),
                        IsCumulative = false,
                        CreatedByUserId = admin?.Id,
                        HotelId = null // sajt-wide
                    },
                    new PriceAdjustment
                    {
                        Name = "Nova godina",
                        PercentageModifier = 25m,
                        StartDate = new DateTime(today.Year, 12, 20),
                        EndDate = new DateTime(today.Year + 1, 1, 5),
                        IsCumulative = false,
                        CreatedByUserId = admin?.Id,
                        HotelId = firstHotelForPricing?.Id // demonstracija hotel-specifičnog pravila
                    });
                await context.SaveChangesAsync();
            }

            var demo = await context.Users.FirstOrDefaultAsync(u => u.Email == "demo@demo.com");
            var ana = await context.Users.FirstOrDefaultAsync(u => u.Email == "ana@demo.com");
            var marko = await context.Users.FirstOrDefaultAsync(u => u.Email == "marko@demo.com");
            var ivan = await context.Users.FirstOrDefaultAsync(u => u.Email == "ivan@demo.com");
            var leo = await context.Users.FirstOrDefaultAsync(u => u.Email == "leo@demo.com");
            // NAMJERNO ne prva soba u cijeloj bazi (bila bi "101" — Blue Sea Hotel, Split, prvi
            // hotel, prva soba) — to je soba koju će vjerovatno prvu otvoriti neko ko testira app
            // (npr. profesor na odbrani), pa ne treba da već ima seed održavanje na sebi (jedan od
            // dva seed zapisa je namjerno NEriješen, da demonstrira "otvoren" status). Umjesto toga
            // uzima sobu iz DRUGOG hotela (Alpine Lodge, Bled), sa fallback-om na prvu ako iz nekog
            // razloga ne postoji.
            var firstRoom = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "203")
                ?? await context.Rooms.FirstOrDefaultAsync();
            var allBookingsForOps = await context.Bookings.OrderBy(b => b.Id).ToListAsync();
            var demoBooking = demo != null
                ? allBookingsForOps.FirstOrDefault(b => b.UserId == demo.Id)
                : null;
            var anaBooking = ana != null
                ? allBookingsForOps.FirstOrDefault(b => b.UserId == ana.Id)
                : null;
            var markoBookings = marko != null
                ? allBookingsForOps.Where(b => b.UserId == marko.Id).ToList()
                : new List<Booking>();
            var ivanBooking = ivan != null
                ? allBookingsForOps.FirstOrDefault(b => b.UserId == ivan.Id)
                : null;

            if (!await context.SupportTickets.AnyAsync() && demo != null && ana != null)
            {
                context.SupportTickets.AddRange(
                    new SupportTicket
                    {
                        UserId = demo.Id,
                        Subject = "Kasni check-in",
                        MessageBody = "Molim potvrdu kasnog dolaska nakon 22h.",
                        Status = SupportTicketStatus.Open,
                        Priority = SupportTicketPriority.Medium
                        // Namjerno bez odgovora — demonstrira i "čeka se odgovor" stanje u UI-ju.
                    },
                    new SupportTicket
                    {
                        UserId = ana.Id,
                        Subject = "Pitanje o parkingu",
                        MessageBody = "Da li hotel nudi besplatan parking?",
                        Status = SupportTicketStatus.Closed,
                        Priority = SupportTicketPriority.Low,
                        AdminResponse = "Da, parking je besplatan za sve goste hotela, nalazi se odmah pored ulaza.",
                        RespondedAt = today.AddDays(-1),
                        RespondedByUserId = leo?.Id
                    });
                await context.SaveChangesAsync();
            }

            if (!await context.RoomMaintenanceLogs.AnyAsync() && firstRoom != null)
            {
                context.RoomMaintenanceLogs.AddRange(
                    new RoomMaintenanceLog
                    {
                        RoomId = firstRoom.Id,
                        ReportedAt = today.AddDays(-3),
                        ResolvedAt = today.AddDays(-2),
                        Description = "Curenje slavine u kupatilu",
                        Cost = 45m,
                        TechnicianName = "Mario K."
                    },
                    new RoomMaintenanceLog
                    {
                        RoomId = firstRoom.Id,
                        ReportedAt = today.AddDays(-1),
                        Description = "Klima ne hladi dovoljno",
                        Cost = 0m,
                        TechnicianName = "Pending"
                    });
                await context.SaveChangesAsync();
            }

            // Referentna/šifarnik tabela kategorija artikala skladišta. Mora postojati PRIJE
            // artikala jer je InventoryItem.InventoryItemCategoryId obavezan FK (isti obrazac
            // kao ServiceCategory/Service).
            if (!await context.InventoryItemCategories.AnyAsync())
            {
                context.InventoryItemCategories.AddRange(
                    new InventoryItemCategory { Name = "Higijena" },
                    new InventoryItemCategory { Name = "Mini bar" },
                    new InventoryItemCategory { Name = "Tekstil" }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.InventoryItems.AnyAsync())
            {
                var higijenaCategory = await context.InventoryItemCategories.FirstAsync(c => c.Name == "Higijena");
                var miniBarCategory = await context.InventoryItemCategories.FirstAsync(c => c.Name == "Mini bar");
                var tekstilCategory = await context.InventoryItemCategories.FirstAsync(c => c.Name == "Tekstil");

                context.InventoryItems.AddRange(
                    new InventoryItem { Name = "Sapun", Unit = "kom", InventoryItemCategoryId = higijenaCategory.Id, MinimumStockLevel = 50 },
                    new InventoryItem { Name = "Mini bar - napici", Unit = "kom", InventoryItemCategoryId = miniBarCategory.Id, MinimumStockLevel = 20 },
                    new InventoryItem { Name = "Peškiri", Unit = "kom", InventoryItemCategoryId = tekstilCategory.Id, MinimumStockLevel = 30 });
                await context.SaveChangesAsync();
            }

            var soap = await context.InventoryItems.FirstOrDefaultAsync(x => x.Name == "Sapun");
            var miniBar = await context.InventoryItems.FirstOrDefaultAsync(x => x.Name == "Mini bar - napici");

            if (!await context.InventoryTransactions.AnyAsync() && leo != null && soap != null && miniBar != null)
            {
                context.InventoryTransactions.AddRange(
                    new InventoryTransaction
                    {
                        InventoryItemId = soap.Id,
                        QuantityChange = 100,
                        TransactionDate = today.AddDays(-7),
                        StaffUserId = leo.Id,
                        Reason = "Ulaz robe - sapuni"
                    },
                    new InventoryTransaction
                    {
                        InventoryItemId = soap.Id,
                        QuantityChange = -20,
                        TransactionDate = today.AddDays(-2),
                        StaffUserId = leo.Id,
                        Reason = "Restocking soba 101-110"
                    },
                    new InventoryTransaction
                    {
                        InventoryItemId = miniBar.Id,
                        QuantityChange = -15,
                        TransactionDate = today,
                        StaffUserId = leo.Id,
                        Reason = "Mini bar potrošnja"
                    });
                await context.SaveChangesAsync();
            }

            // Plaćanja + audit log po plaćanju. Zadnja (Ivan/Mostar, Confirmed) namjerno ostaje
            // Pending da postoji stvaran primjer "neplaćene" rezervacije za mobile "Plati ponovo" tok.
            if (!await context.Payments.AnyAsync())
            {
                var paymentSeeds = new List<(Booking Booking, User User, PaymentStatus Status, string Suffix)>();
                if (demo != null && demoBooking != null) paymentSeeds.Add((demoBooking, demo, PaymentStatus.Completed, "demo"));
                if (ana != null && anaBooking != null) paymentSeeds.Add((anaBooking, ana, PaymentStatus.Completed, "ana"));
                if (marko != null && markoBookings.Count > 0) paymentSeeds.Add((markoBookings[0], marko, PaymentStatus.Completed, "marko1"));
                if (marko != null && markoBookings.Count > 1) paymentSeeds.Add((markoBookings[1], marko, PaymentStatus.Completed, "marko2"));
                if (ivan != null && ivanBooking != null) paymentSeeds.Add((ivanBooking, ivan, PaymentStatus.Pending, "ivan"));

                var payments = paymentSeeds.Select(s => new Payment
                {
                    UserId = s.User.Id,
                    BookingId = s.Booking.Id,
                    Amount = s.Booking.TotalPrice,
                    PaymentMethod = PaymentMethod.Stripe,
                    Status = s.Status,
                    Currency = "EUR",
                    TransactionId = s.Status == PaymentStatus.Completed ? $"pi_seed_{s.Suffix}" : null,
                    CheckoutId = $"cs_seed_{s.Suffix}",
                    Description = $"Uplata za rezervaciju #{s.Booking.Id}",
                    ProcessedAt = s.Status == PaymentStatus.Completed ? today.AddDays(-1) : null
                }).ToList();

                if (payments.Count > 0)
                {
                    context.Payments.AddRange(payments);
                    await context.SaveChangesAsync();

                    var auditLogs = new List<PaymentAuditLog>();
                    for (var i = 0; i < payments.Count; i++)
                    {
                        var payment = payments[i];
                        var seed = paymentSeeds[i];
                        auditLogs.Add(payment.Status == PaymentStatus.Completed
                            ? new PaymentAuditLog
                            {
                                PaymentId = payment.Id,
                                FromStatus = PaymentStatus.Pending,
                                ToStatus = PaymentStatus.Completed,
                                Action = "PaymentCompleted",
                                Details = "Stripe checkout session uspješno završena",
                                InitiatedByUserId = seed.User.Id,
                                AttemptedAt = payment.ProcessedAt ?? today
                            }
                            : new PaymentAuditLog
                            {
                                PaymentId = payment.Id,
                                FromStatus = PaymentStatus.Pending,
                                ToStatus = PaymentStatus.Pending,
                                Action = "PaymentInitiated",
                                Details = "Stripe checkout session kreirana, čeka se uplata",
                                InitiatedByUserId = seed.User.Id,
                                AttemptedAt = today
                            });
                    }
                    context.PaymentAuditLogs.AddRange(auditLogs);
                    await context.SaveChangesAsync();
                }
            }

            // Notifikacije vezane za rezervacije/plaćanja iznad.
            if (!await context.Notifications.AnyAsync())
            {
                var notifications = new List<Notification>();
                if (demo != null && demoBooking != null)
                {
                    notifications.Add(new Notification
                    {
                        UserId = demo.Id,
                        BookingId = demoBooking.Id,
                        Title = "Rezervacija potvrđena",
                        Message = $"Vaša rezervacija #{demoBooking.Id} je uspješno plaćena i potvrđena.",
                        Type = "PaymentReceived",
                        Priority = "Normal",
                        IsRead = true,
                        SentDate = today.AddDays(-1),
                        ReadDate = today.AddDays(-1)
                    });
                }
                if (ana != null && anaBooking != null)
                {
                    notifications.Add(new Notification
                    {
                        UserId = ana.Id,
                        BookingId = anaBooking.Id,
                        Title = "Rezervacija potvrđena",
                        Message = $"Vaša rezervacija #{anaBooking.Id} je uspješno plaćena i potvrđena.",
                        Type = "PaymentReceived",
                        Priority = "Normal",
                        IsRead = false,
                        SentDate = today.AddDays(-1)
                    });
                }
                if (ivan != null && ivanBooking != null)
                {
                    notifications.Add(new Notification
                    {
                        UserId = ivan.Id,
                        BookingId = ivanBooking.Id,
                        Title = "Plaćanje na čekanju",
                        Message = $"Rezervacija #{ivanBooking.Id} čeka uplatu. Dovršite plaćanje kako biste zadržali rezervaciju.",
                        Type = "PaymentPending",
                        Priority = "High",
                        IsRead = false,
                        SentDate = today
                    });
                }
                if (notifications.Count > 0)
                {
                    context.Notifications.AddRange(notifications);
                    await context.SaveChangesAsync();
                }
            }

            // Historija promjena statusa rezervacija (Pending -> trenutni status).
            if (!await context.BookingStatusHistories.AnyAsync() && allBookingsForOps.Count > 0)
            {
                var admin2 = admin ?? await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@demo.com");
                var histories = allBookingsForOps.Select(b => new BookingStatusHistory
                {
                    BookingId = b.Id,
                    FromStatus = BookingStatus.Pending,
                    ToStatus = b.Status,
                    ChangeDate = b.CreatedAt,
                    Reason = b.Status == BookingStatus.Cancelled ? "Otkazano od strane gosta" : "Automatska potvrda rezervacije",
                    ChangedByUserId = admin2?.Id
                }).ToList();

                context.BookingStatusHistories.AddRange(histories);
                await context.SaveChangesAsync();
            }
        }
    }
}


