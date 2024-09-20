using System.Collections.Immutable;

namespace FamiTracker;

public class Pattern
{
    private Event[] _Events;
    public byte Id;

    public Pattern(byte Id, byte patternSize)
    {
        this.Id = Id;
        _Events = new Event[patternSize];
    }

    public ImmutableList<Event> Events
    {
        get { return _Events.ToImmutableList(); }
    }

    public void SetEvent(byte Index, Event? patternEvent) => _Events[Index] = patternEvent;
}