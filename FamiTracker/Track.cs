using System.Collections.Immutable;

namespace FamiTracker;

public class Track
{
    private List<Pattern> _Patterns = new List<Pattern>();
    private byte PatternSize;
    public byte[] Playlist;

    public Track(byte playListSize, byte patternSize)
    {
        Playlist = new byte[playListSize];
        PatternSize = patternSize;
    }

    public ImmutableList<Pattern> Patterns
    {
        get { return _Patterns.ToImmutableList(); }
    }

    public void AddPattern(Event[] pattern) => _Patterns.Add(new Pattern(PatternSize));

    public ImmutableList<Pattern> GetPatternSequence()
    {
        List<Pattern> result = new List<Pattern>();
        foreach (byte index in Playlist)
        {
            result.Add(Patterns.ElementAt(index));
        }

        return result.ToImmutableList();
    }
}