namespace FamiTracker;

public class Project
{
    public Globals Globals { get; set; }
    public Metadata Metadata { get; set; }
    public List<Instrument> Instruments { get; } = new List<Instrument>();
    public List<Song> Songs { get; } = new List<Song>();
}