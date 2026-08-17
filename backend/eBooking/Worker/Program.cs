using Application.Configuration;
using Application.Messaging.Configuration;
using Application.Services;
using Application.Services.PaymentProviders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Data;
using Persistence.Interfaces;
using Persistence.Models;
using Persistence.Repositories;
using Persistence.Services;
using Worker.Consumers;
using BookingService = Application.Services.BookingService;

var builder = Host.CreateApplicationBuilder(args);

// --- Baza (ista konekcija kao API, ali Worker ne pokreće migracije/seed — to radi API pri startu) ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// --- Repozitoriji (identično kao API/Program.cs — Worker treba isti DI graf da može
// konstruisati BookingService/PaymentService/NotificationService itd.) ---
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRepository<RoomMaintenanceLog>, RoomMaintenanceLogRepository>();
builder.Services.AddScoped<IRepository<SupportTicket>, SupportTicketRepository>();
builder.Services.AddScoped<IRepository<InventoryTransaction>, InventoryTransactionRepository>();
builder.Services.AddScoped<IRepository<LoyaltyPointsRedemption>, LoyaltyPointsRedemptionRepository>();
builder.Services.AddScoped<IRepository<Service>, ServiceRepository>();
builder.Services.AddScoped<IRepository<Room>, RoomRepository>();

// --- Application servisi (identično kao API/Program.cs) ---
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingStatusHistoryService, BookingStatusHistoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentAuditLogService, PaymentAuditLogService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IRoomMaintenanceLogService, RoomMaintenanceLogService>();
builder.Services.AddScoped<IPriceAdjustmentService, PriceAdjustmentService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
builder.Services.AddScoped<ILoyaltyPointsRedemptionService, LoyaltyPointsRedemptionService>();
builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
builder.Services.AddScoped<IRoomService, RoomService>();

// Queries (read-only)
builder.Services.AddScoped<Application.Queries.IBookingQueries, Application.Queries.BookingQueries>();
builder.Services.AddScoped<Application.Queries.IServiceQueries, Application.Queries.ServiceQueries>();

// --- Plaćanja (PaymentService je posredna zavisnost BookingService-a; Worker je ne poziva
// direktno za checkout, ali DI mora znati sastaviti kompletan graf) ---
builder.Services.Configure<PaymentOptions>(builder.Configuration.GetSection(PaymentOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpClient("PayPalApi", (sp, client) =>
{
    var baseUrl = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<PaymentOptions>>().Value.PayPal.BaseUrl;
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});
builder.Services.AddScoped<IWebhookEventDedupService, WebhookEventDedupService>();
builder.Services.AddScoped<PayPalPaymentProvider>();
builder.Services.AddScoped<StripePaymentProvider>();
builder.Services.AddScoped<IPaymentGatewayProvider>(sp => sp.GetRequiredService<PayPalPaymentProvider>());
builder.Services.AddScoped<IPaymentGatewayProvider>(sp => sp.GetRequiredService<StripePaymentProvider>());

// --- MassTransit + RabbitMQ: Worker konzumira poslovne (async) evente i piše u bazu preko
// Application servisa iznad (npr. kreira Notification zapis, potvrđuje Booking nakon plaćanja).
// NotificationCreatedConsumer OSTAJE u API-ju (treba mu SignalR IHubContext iz istog procesa). ---
builder.Services.AddMessaging(
    builder.Configuration,
    configureConsumers: x =>
    {
        x.AddConsumer<BookingConfirmedConsumer>();
        x.AddConsumer<BookingUpdatedConsumer>();
        x.AddConsumer<PaymentCompletedConsumer>();
        x.AddConsumer<UpcomingCheckInReminderConsumer>();
    },
    configureEndpoints: (ctx, cfg) =>
    {
        cfg.ReceiveEndpoint("booking-confirmed-queue", e => e.ConfigureConsumer<BookingConfirmedConsumer>(ctx));
        cfg.ReceiveEndpoint("booking-updated-queue", e => e.ConfigureConsumer<BookingUpdatedConsumer>(ctx));
        cfg.ReceiveEndpoint("payment-completed-queue", e => e.ConfigureConsumer<PaymentCompletedConsumer>(ctx));
        cfg.ReceiveEndpoint("upcoming-checkin-reminder-queue", e => e.ConfigureConsumer<UpcomingCheckInReminderConsumer>(ctx));
    });

var host = builder.Build();
host.Run();
