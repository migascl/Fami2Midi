namespace FamiTracker;

public class Project
{
    public static Globals Globals { get; set; }
    public static Metadata Metadata { get; set; }
    public List<Instrument> Instruments { get; } = new List<Instrument>();
    public List<Song> Songs { get; } = new List<Song>();
}