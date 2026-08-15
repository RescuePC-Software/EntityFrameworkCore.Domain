using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using RescuePC.Software.EntityFrameworkCore.Domain.Interceptors;

namespace RescuePC.Software.EntityFrameworkCore.Domain.UnitTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPublishEventsInterceptor_RegistersPublishEventsInterceptorAsIInterceptor()
    {
        var services = new ServiceCollection();

        services.AddPublishEventsInterceptor();

        var descriptor = Assert.Single(services);
        Assert.Equal(typeof(PublishEventsInterceptor), descriptor.ServiceType);
        Assert.Equal(typeof(PublishEventsInterceptor), descriptor.ImplementationType);
    }

    [Fact]
    public void AddPublishEventsInterceptor_RegistersAsScoped()
    {
        var services = new ServiceCollection();

        services.AddPublishEventsInterceptor();

        var descriptor = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddPublishEventsInterceptor_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddPublishEventsInterceptor();

        Assert.Same(services, result);
    }
}
