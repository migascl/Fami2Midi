namespace FamiTracker;

public class Song
{
    public byte Frames { get; set; } = 1;
    public byte Rows { get; set; } = 64;
    public byte Speed { get; set; } = 6;
    public byte Tempo { get; set; } = 150;
    public string Title { get; set; } = "New song";
    public List<Track> Tracks { get; } = new List<Track>();
}