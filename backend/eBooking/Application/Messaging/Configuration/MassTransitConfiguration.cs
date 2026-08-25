using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Messaging.Configuration
{
    /// <summary>
    /// Zajednička MassTransit/RabbitMQ konfiguracija za API (publisher, + eventualno tanki
    /// realtime relay konzumer) i Worker (glavni konzument poslovnih async zadataka).
    /// Svaki servis registruje SAMO svoje konzumere kroz <paramref name="configureConsumers"/>
    /// i <paramref name="configureEndpoints"/> — API i Worker su namjerno odvojeni procesi/kontejneri
    /// koji dijele istu RabbitMQ konekciju/konfiguraciju, ali ne i isti skup konzumera.
    /// </summary>
    public static class MassTransitConfiguration
    {
        public static IServiceCollection AddMessaging(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<IBusRegistrationConfigurator> configureConsumers,
            Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureEndpoints)
        {
            services.AddMassTransit(x =>
            {
                configureConsumers(x);

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    var host = configuration["Rabbit:Host"] ?? "rabbitmq";
                    var user = configuration["Rabbit:User"] ?? "guest";
                    var pass = configuration["Rabbit:Pass"] ?? "guest";
                    cfg.Host(host, h => { h.Username(user); h.Password(pass); });

                    // Bus-wide retry politika (uputa Dodatak A.1: "implementirati retry logiku sa
                    // eksponencijalnim backoff-om, npr. 1s -> 2s -> 4s -> 8s"). Primjenjuje se na
                    // SVE receive endpointe registrovane niže (API i Worker konzumeri) — bez ovoga
                    // je greška u konzumeru išla direktno u error queue nakon prvog pokušaja.
                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(4),
                        TimeSpan.FromSeconds(8)));

                    configureEndpoints(ctx, cfg);
                });
            });

            return services;
        }

        /// <summary>
        /// API koristi samo publish (IPublishEndpoint/IBus) + NotificationCreatedConsumer,
        /// jer taj konzument mora živjeti u istom procesu kao SignalR hub (IHubContext) da bi
        /// mogao realtime pushati već povezanim klijentima. Nema poslovne (DB) logike.
        /// </summary>
        public static IServiceCollection AddApiMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddMessaging(
                configuration,
                configureConsumers: x =>
                {
                    x.AddConsumer<Application.Messaging.Consumers.NotificationCreatedConsumer>();
                },
                configureEndpoints: (ctx, cfg) =>
                {
                    cfg.ReceiveEndpoint("notification-created-queue", e =>
                    {
                        e.ConfigureConsumer<Application.Messaging.Consumers.NotificationCreatedConsumer>(ctx);
                    });
                });
        }
    }
}
