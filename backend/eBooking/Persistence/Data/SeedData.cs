using Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Models;
using Contracts.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context, IServiceProvider services)
        {
            // Hoteli + sobe + servisi (osnovni seed)
            if (!await context.Hotels.AnyAsync())
            {
                var hotels = new List<Hotel>
                {
                    new Hotel { Name = "Blue Sea Hotel", Address = "Riviera 1", City = "Split", Country = "HR", PhoneNumber = "+385 21 123 456", Email = "info@bluesea.hr", Description = "Hotel uz more sa predivnim pogledom", ImageUrl = "https://i.postimg.cc/0NVL5tbt/hotel2.jpg" },
                    new Hotel { Name = "Alpine Lodge", Address = "Dolomiti 12", City = "Bled", Country = "SI", PhoneNumber = "+386 4 987 654", Email = "info@alpinelodge.si", Description = "Planinski ugođaj i wellness", ImageUrl = "https://i.postimg.cc/y8qp7Mn6/hotel1.webp" },
                    new Hotel { Name = "City Center Inn", Address = "King St 10", City = "Sarajevo", Country = "BA", PhoneNumber = "+387 33 111 222", Email = "info@citycenter.ba", Description = "U srcu grada, blizu svih atrakcija", ImageUrl = "https://i.postimg.cc/yYCrKqL1/hotel3.jpg" },
                    new Hotel { Name = "Riverside Retreat", Address = "Obala 5", City = "Mostar", Country = "BA", PhoneNumber = "+387 36 555 777", Email = "hello@riverside.ba", Description = "Ugodan boravak uz rijeku", ImageUrl = "https://i.postimg.cc/HnD3T54Z/hotel4.jpg" },
                    new Hotel { Name = "Metropolis Hotel", Address = "Main Ave 44", City = "Zagreb", Country = "HR", PhoneNumber = "+385 1 222 333", Email = "contact@metropolis.hr", Description = "Moderan gradski hotel", ImageUrl = "https://i.postimg.cc/bNPLMxvd/hotel5.png" },
                };
                context.Hotels.AddRange(hotels);
                await context.SaveChangesAsync();

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
                var servicesList = new List<Service>();
                foreach (var h in hotels)
                {
                    servicesList.AddRange(new[]
                    {
                        new Service { HotelId = h.Id, Name = "Spa paket", Description = "Sauna i masaža 60min", Category = "Spa", Price = 30, IsAvailable = true, IsActive = true },
                        new Service { HotelId = h.Id, Name = "Doručak", Description = "Buffet doručak", Category = "Food", Price = 8, IsAvailable = true, IsActive = true },
                        new Service { HotelId = h.Id, Name = "Aerodrom shuttle", Description = "Prevoz do aerodroma", Category = "Transport", Price = 25, IsAvailable = true, IsActive = true },
                    });
                }
                context.Services.AddRange(servicesList);
                await context.SaveChangesAsync();
            }

            // Users + primjeri rezervacija i recenzija (za preporuke)
            if (!await context.Users.AnyAsync())
            {
                var passwordService = services.GetService<IPasswordService>();
                var admin = new User { Username = "admin", Email = "admin@demo.com", FirstName = "Admin", LastName = "User", Role = UserRole.Admin, IsActive = true };
                var demo = new User { Username = "demo", Email = "demo@demo.com", FirstName = "Demo", LastName = "User", Role = UserRole.Guest, IsActive = true };
                var ana = new User { Username = "ana", Email = "ana@demo.com", FirstName = "Ana", LastName = "Anić", Role = UserRole.Guest, IsActive = true };
                var marko = new User { Username = "marko", Email = "marko@demo.com", FirstName = "Marko", LastName = "Marković", Role = UserRole.Guest, IsActive = true };
                var ivan = new User { Username = "ivan", Email = "ivan@demo.com", FirstName = "Ivan", LastName = "Ivić", Role = UserRole.Guest, IsActive = true };
                var leo = new User { Username = "leo", Email = "leo@demo.com", FirstName = "Leo", LastName = "Leić", Role = UserRole.Employee, IsActive = true };

                if (passwordService != null)
                {
                    admin.PasswordHash = passwordService.HashPassword("Admin123!");
                    demo.PasswordHash = passwordService.HashPassword("Demo123!");
                    ana.PasswordHash = passwordService.HashPassword("Ana123!");
                    marko.PasswordHash = passwordService.HashPassword("Marko123!");
                    ivan.PasswordHash = passwordService.HashPassword("Ivan123!");
                    leo.PasswordHash = passwordService.HashPassword("Leo123!");
                }
                context.Users.AddRange(admin, demo, ana, marko, ivan, leo);
                await context.SaveChangesAsync();

                var hotelsAll = await context.Hotels.ToListAsync();
                var roomsAll = await context.Rooms.ToListAsync();
                var servicesAll = await context.Services.ToListAsync();

                Room? RoomForHotel(int hotelId) => roomsAll.FirstOrDefault(r => r.HotelId == hotelId);
                Service? ServiceForHotel(int hotelId, string category) => servicesAll.FirstOrDefault(s => s.HotelId == hotelId && s.Category == category);

                var today = DateTime.UtcNow.Date;
                var bookings = new List<Booking>();

                // Demo voli more (Split), često uzima spa i doručak
                var splitHotel = hotelsAll.FirstOrDefault(h => h.City == "Split");
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
                var bledHotel = hotelsAll.FirstOrDefault(h => h.City == "Bled");
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
                var zagrebHotel = hotelsAll.FirstOrDefault(h => h.City == "Zagreb");
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

                var sarajevoHotel = hotelsAll.FirstOrDefault(h => h.City == "Sarajevo");
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
                var mostarHotel = hotelsAll.FirstOrDefault(h => h.City == "Mostar");
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
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Split").Id, UserId = demo.Id, Rating = 5, Title = "Odlično more", Comment = "Prekrasan pogled na more", ReviewDate = today.AddDays(-15), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Zagreb").Id, UserId = demo.Id, Rating = 4, Title = "Dobar gradski hotel", Comment = "Moderan i udoban", ReviewDate = today.AddDays(-12), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Bled").Id, UserId = demo.Id, Rating = 3, Title = "Planinski ugođaj", Comment = "Nije moj stil", ReviewDate = today.AddDays(-10), IsApproved = true });
                
                // Ana - voli planine i prirodu (srednje ocjene)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Bled").Id, UserId = ana.Id, Rating = 5, Title = "Prekrasne planine", Comment = "Odličan wellness", ReviewDate = today.AddDays(-9), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Mostar").Id, UserId = ana.Id, Rating = 4, Title = "Lijep grad", Comment = "Ugodan boravak", ReviewDate = today.AddDays(-7), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Split").Id, UserId = ana.Id, Rating = 2, Title = "Previše turista", Comment = "Gradski hotel", ReviewDate = today.AddDays(-5), IsApproved = true });
                
                // Ivan - voli historijske gradove (visoke ocjene za historijske gradove)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Mostar").Id, UserId = ivan.Id, Rating = 5, Title = "Historijski grad", Comment = "Prelijep most", ReviewDate = today.AddDays(-3), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Sarajevo").Id, UserId = ivan.Id, Rating = 4, Title = "Baščaršija", Comment = "Odlična kultura", ReviewDate = today.AddDays(-1), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Zagreb").Id, UserId = ivan.Id, Rating = 3, Title = "Moderan grad", Comment = "Ok, ali nije historijski", ReviewDate = today.AddDays(-8), IsApproved = true });
                
                // Marko - voli luksuz i visoke ocjene (sličan Demo-u)
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Split").Id, UserId = marko.Id, Rating = 5, Title = "Luksuzan boravak", Comment = "Odličan spa", ReviewDate = today.AddDays(-6), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Zagreb").Id, UserId = marko.Id, Rating = 4, Title = "Moderan luksuz", Comment = "Dobar standard", ReviewDate = today.AddDays(-4), IsApproved = true });
                reviews.Add(new Review { HotelId = hotelsAll.First(h => h.City == "Bled").Id, UserId = marko.Id, Rating = 4, Title = "Planinski luksuz", Comment = "Odličan wellness", ReviewDate = today.AddDays(-2), IsApproved = true });
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }
        }
    }
}


