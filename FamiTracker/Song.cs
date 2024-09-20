namespace FamiTracker;

public class Song
{
    public byte Frames { get; set; }
    public byte Rows { get; set; }
    public byte Speed { get; set; }
    public byte Tempo { get; set; }
    public string Title { get; set; } = "New song";
    public List<Track> Tracks { get; } = new List<Track>();
}