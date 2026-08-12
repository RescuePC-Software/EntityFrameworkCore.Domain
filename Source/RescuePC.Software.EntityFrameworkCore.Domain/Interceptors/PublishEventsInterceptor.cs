using Microsoft.EntityFrameworkCore.Diagnostics;
using RescuePC.Software.Domain;
using RescuePC.Software.Domain.Event;

namespace RescuePC.Software.EntityFrameworkCore.Domain.Interceptors;

public class PublishEventsInterceptor : SaveChangesInterceptor
{
    private readonly IEventBus _eventBus;

    public PublishEventsInterceptor(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return await base.SavedChangesAsync(eventData, result, cancellationToken);
        var aggregateRoots = eventData.Context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(e => e.GetDomainEvents().Count > 0)
            .ToList();

        var events = aggregateRoots
            .SelectMany(e => e.GetDomainEvents())
            .ToList();

        aggregateRoots.ForEach(e => e.ClearDomainEvents());

        foreach (var @event in events)
        {
            await _eventBus.PublishAsync(@event, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}