using Microsoft.EntityFrameworkCore;
using NSubstitute;
using RescuePC.Software.Domain;
using RescuePC.Software.Domain.Event;
using RescuePC.Software.EntityFrameworkCore.Domain.Interceptors;

namespace RescuePC.Software.EntityFrameworkCore.Domain.UnitTests.Interceptors;

public class PublishEventsInterceptorTests
{
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();

    private TestDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new PublishEventsInterceptor(_eventBus))
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task SavedChangesAsync_WhenAggregateRootHasEvents_PublishesAllEvents()
    {
        await using var context = CreateDbContext();
        var domainEvent = Substitute.For<IEvent>();
        var entity = new TestAggregateRoot();
        entity.AddDomainEvent(domainEvent);

        context.Aggregates.Add(entity);
        await context.SaveChangesAsync();

        await _eventBus.Received(1).PublishAsync(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_WhenAggregateRootHasMultipleEvents_PublishesEachEvent()
    {
        await using var context = CreateDbContext();
        var firstEvent = Substitute.For<IEvent>();
        var secondEvent = Substitute.For<IEvent>();
        var entity = new TestAggregateRoot();
        entity.AddDomainEvent(firstEvent);
        entity.AddDomainEvent(secondEvent);

        context.Aggregates.Add(entity);
        await context.SaveChangesAsync();

        await _eventBus.Received(1).PublishAsync(firstEvent, Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(secondEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SavedChangesAsync_WhenAggregateRootHasEvents_ClearsDomainEventsAfterPublishing()
    {
        await using var context = CreateDbContext();
        var entity = new TestAggregateRoot();
        entity.AddDomainEvent(Substitute.For<IEvent>());

        context.Aggregates.Add(entity);
        await context.SaveChangesAsync();

        Assert.Empty(entity.GetDomainEvents());
    }

    [Fact]
    public async Task SavedChangesAsync_WhenAggregateRootHasNoEvents_DoesNotPublishAnyEvents()
    {
        await using var context = CreateDbContext();
        var entity = new TestAggregateRoot();

        context.Aggregates.Add(entity);
        await context.SaveChangesAsync();

        await _eventBus.DidNotReceiveWithAnyArgs().PublishAsync<IEvent>(default!, default);
    }

    [Fact]
    public async Task SavedChangesAsync_WhenMultipleAggregateRootsHaveEvents_PublishesEventsFromAll()
    {
        await using var context = CreateDbContext();
        var firstEvent = Substitute.For<IEvent>();
        var secondEvent = Substitute.For<IEvent>();

        var firstEntity = new TestAggregateRoot();
        firstEntity.AddDomainEvent(firstEvent);

        var secondEntity = new TestAggregateRoot();
        secondEntity.AddDomainEvent(secondEvent);

        context.Aggregates.AddRange(firstEntity, secondEntity);
        await context.SaveChangesAsync();

        await _eventBus.Received(1).PublishAsync(firstEvent, Arg.Any<CancellationToken>());
        await _eventBus.Received(1).PublishAsync(secondEvent, Arg.Any<CancellationToken>());
    }
}

internal class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestAggregateRoot> Aggregates => Set<TestAggregateRoot>();
}

internal class TestAggregateRoot : IAggregateRoot
{
    private readonly List<IEvent> _domainEvents = [];

    public int Id { get; private set; }

    public void AddDomainEvent(IEvent domainEvent) => _domainEvents.Add(domainEvent);

    public IReadOnlyCollection<IEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
}
