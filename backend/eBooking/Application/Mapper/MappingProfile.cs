using AutoMapper;
using Contracts.DTOs;
using Persistence.Models;

namespace Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Hotel mappings
            CreateMap<Hotel, HotelDto>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City != null ? src.City.Name : string.Empty))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.City != null && src.City.Country != null ? src.City.Country.Name : string.Empty));
            CreateMap<CreateHotelDto, Hotel>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateHotelDto, Hotel>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Country / City mappings (referentne tabele — zamjena za slobodan tekst na Hotelu)
            CreateMap<Country, CountryDto>();
            CreateMap<CreateCountryDto, Country>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateCountryDto, Country>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<City, CityDto>()
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.Name : string.Empty));
            CreateMap<CreateCityDto, City>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateCityDto, City>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Booking mappings
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.BookingServices))
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : string.Empty))
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Room != null && src.Room.Hotel != null ? src.Room.Hotel.Name : null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null
                    ? (!string.IsNullOrWhiteSpace(src.User.FirstName) || !string.IsNullOrWhiteSpace(src.User.LastName)
                        ? $"{src.User.FirstName} {src.User.LastName}".Trim()
                        : src.User.Username)
                    : string.Empty));
            CreateMap<BookingService, BookingServiceItemDto>()
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.Name : null));
            CreateMap<CreateBookingDto, Booking>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Contracts.Enums.BookingStatus.Pending));
            CreateMap<UpdateBookingDto, Booking>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // Room mappings
            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name));
            CreateMap<CreateRoomDto, Room>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateRoomDto, Room>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Service mappings
            CreateMap<Service, ServiceDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.ServiceCategory != null ? src.ServiceCategory.Name : string.Empty));
            CreateMap<CreateServiceDto, Service>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateServiceDto, Service>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // ServiceCategory mappings (referentna tabela — zamjena za slobodan tekst na Service.Category)
            CreateMap<ServiceCategory, ServiceCategoryDto>();
            CreateMap<CreateServiceCategoryDto, ServiceCategory>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateServiceCategoryDto, ServiceCategory>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Review mappings
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : null));
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.ReviewDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateReviewDto, Review>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Notification mappings
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username));
            CreateMap<CreateNotificationDto, Notification>()
                .ForMember(dest => dest.SentDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateNotificationDto, Notification>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // BookingStatusHistory mappings
            CreateMap<BookingStatusHistory, BookingStatusHistoryDto>()
                .ForMember(dest => dest.ChangedByUserName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.Username : null));
            CreateMap<CreateBookingStatusHistoryDto, BookingStatusHistory>()
                .ForMember(dest => dest.ChangeDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateBookingStatusHistoryDto, BookingStatusHistory>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Payment mappings
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.BookingReference, opt => opt.MapFrom(src => $"BK-{src.Booking.Id:D6}"));
            CreateMap<CreatePaymentDto, Payment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Contracts.Enums.PaymentStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<CreateHostedCheckoutDto, Payment>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Contracts.Enums.PaymentStatus.Pending))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdatePaymentDto, Payment>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // PaymentAuditLog mappings
            CreateMap<PaymentAuditLog, PaymentAuditLogDto>()
                .ForMember(dest => dest.InitiatedByUserName, opt => opt.MapFrom(src => src.InitiatedByUser != null ? src.InitiatedByUser.Username : null));
            CreateMap<CreatePaymentAuditLogDto, PaymentAuditLog>()
                .ForMember(dest => dest.AttemptedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdatePaymentAuditLogDto, PaymentAuditLog>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // RoomMaintenanceLog mappings
            CreateMap<RoomMaintenanceLog, RoomMaintenanceLogDto>()
                .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomNumber : string.Empty));
            CreateMap<CreateRoomMaintenanceLogDto, RoomMaintenanceLog>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateRoomMaintenanceLogDto, RoomMaintenanceLog>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // PriceAdjustment mappings
            CreateMap<PriceAdjustment, PriceAdjustmentDto>()
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null
                    ? (!string.IsNullOrWhiteSpace(src.CreatedByUser.FirstName) || !string.IsNullOrWhiteSpace(src.CreatedByUser.LastName)
                        ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}".Trim()
                        : src.CreatedByUser.Username)
                    : string.Empty))
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel != null ? src.Hotel.Name : string.Empty));
            CreateMap<CreatePriceAdjustmentDto, PriceAdjustment>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdatePriceAdjustmentDto, PriceAdjustment>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // InventoryItem mappings (referentna tabela)
            CreateMap<InventoryItem, InventoryItemDto>();
            CreateMap<CreateInventoryItemDto, InventoryItem>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateInventoryItemDto, InventoryItem>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // InventoryTransaction mappings
            CreateMap<InventoryTransaction, InventoryTransactionDto>()
                .ForMember(dest => dest.InventoryItemName, opt => opt.MapFrom(src => src.InventoryItem != null ? src.InventoryItem.Name : string.Empty))
                .ForMember(dest => dest.InventoryItemUnit, opt => opt.MapFrom(src => src.InventoryItem != null ? src.InventoryItem.Unit : string.Empty))
                .ForMember(dest => dest.StaffUserName, opt => opt.MapFrom(src => src.StaffUser != null
                    ? (!string.IsNullOrWhiteSpace(src.StaffUser.FirstName) || !string.IsNullOrWhiteSpace(src.StaffUser.LastName)
                        ? $"{src.StaffUser.FirstName} {src.StaffUser.LastName}".Trim()
                        : src.StaffUser.Username)
                    : string.Empty));
            CreateMap<CreateInventoryTransactionDto, InventoryTransaction>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateInventoryTransactionDto, InventoryTransaction>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // LoyaltyPointsEarned mappings
            CreateMap<LoyaltyPointsEarned, LoyaltyPointsEarnedDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null
                    ? (!string.IsNullOrWhiteSpace(src.User.FirstName) || !string.IsNullOrWhiteSpace(src.User.LastName)
                        ? $"{src.User.FirstName} {src.User.LastName}".Trim()
                        : src.User.Username)
                    : string.Empty))
                .ForMember(dest => dest.BookingLabel, opt => opt.MapFrom(src =>
                    src.Booking != null && src.Booking.Room != null
                        ? $"BK-{src.Booking.Id:D6} · Soba {src.Booking.Room.RoomNumber}"
                        : (src.BookingId != null ? $"BK-{src.BookingId:D6}" : null)));
            CreateMap<CreateLoyaltyPointsEarnedDto, LoyaltyPointsEarned>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateLoyaltyPointsEarnedDto, LoyaltyPointsEarned>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // SupportTicket mappings
            CreateMap<SupportTicket, SupportTicketDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null
                    ? (!string.IsNullOrWhiteSpace(src.User.FirstName) || !string.IsNullOrWhiteSpace(src.User.LastName)
                        ? $"{src.User.FirstName} {src.User.LastName}".Trim()
                        : src.User.Username)
                    : string.Empty))
                .ForMember(dest => dest.RespondedByUserName, opt => opt.MapFrom(src => src.RespondedByUser != null
                    ? (!string.IsNullOrWhiteSpace(src.RespondedByUser.FirstName) || !string.IsNullOrWhiteSpace(src.RespondedByUser.LastName)
                        ? $"{src.RespondedByUser.FirstName} {src.RespondedByUser.LastName}".Trim()
                        : src.RespondedByUser.Username)
                    : null));
            CreateMap<CreateSupportTicketDto, SupportTicket>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            // RespondedAt/RespondedByUserId se NAMJERNO ne mapiraju ovdje — postavljaju se
            // isključivo server-side u SupportTicketsController.Update (SetResponseMetadataAsync),
            // da klijent ne može sam sebi "potpisati" odgovor kao osoblje.
            CreateMap<UpdateSupportTicketDto, SupportTicket>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
