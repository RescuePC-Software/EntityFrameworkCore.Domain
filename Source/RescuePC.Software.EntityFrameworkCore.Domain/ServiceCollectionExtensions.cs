using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using RescuePC.Software.EntityFrameworkCore.Domain.Interceptors;

namespace RescuePC.Software.EntityFrameworkCore.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPublishEventsInterceptor(this IServiceCollection services)
    {
        services.AddScoped<IInterceptor, PublishEventsInterceptor>();
        return services;
    }
}
