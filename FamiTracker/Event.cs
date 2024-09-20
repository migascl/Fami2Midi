namespace FamiTracker;

public class Event
{
    public List<Effect> Effects = new List<Effect>();
    public Instrument? Instrument;
    public NoteType? Kind;
    public byte? Value;
    public byte? Volume;
}