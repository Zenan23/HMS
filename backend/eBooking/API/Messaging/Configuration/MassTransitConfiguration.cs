using API.Messaging.Consumers;
using MassTransit;

namespace API.Messaging.Configuration
{
    public static class MassTransitConfiguration
    {
        public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<BookingUpdatedConsumer>();
                x.AddConsumer<BookingCreatedConsumer>();
                x.AddConsumer<PaymentCompletedConsumer>();
                x.AddConsumer<UpcomingCheckInReminderConsumer>();
                x.AddConsumer<NotificationCreatedConsumer>();

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var host = configuration["Rabbit:Host"] ?? "rabbitmq";
                    var user = configuration["Rabbit:User"] ?? "guest";
                    var pass = configuration["Rabbit:Pass"] ?? "guest";
                    cfg.Host(host, h => { h.Username(user); h.Password(pass); });

                    cfg.ReceiveEndpoint("booking-updated-queue", e => { e.ConfigureConsumer<BookingUpdatedConsumer>(ctx); });
                    cfg.ReceiveEndpoint("booking-created-queue", e => { e.ConfigureConsumer<BookingCreatedConsumer>(ctx); });
                    cfg.ReceiveEndpoint("payment-completed-queue", e => { e.ConfigureConsumer<PaymentCompletedConsumer>(ctx); });
                    cfg.ReceiveEndpoint("upcoming-checkin-reminder-queue", e => { e.ConfigureConsumer<UpcomingCheckInReminderConsumer>(ctx); });
                    cfg.ReceiveEndpoint("notification-created-queue", e => { e.ConfigureConsumer<NotificationCreatedConsumer>(ctx); });
                });
            });

            return services;
        }
    }
}


