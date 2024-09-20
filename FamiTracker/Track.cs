using System.Collections.Immutable;

namespace FamiTracker;

public class Track
{
    private List<Pattern> _Patterns = new List<Pattern>();
    public byte[] Framelist;
    private byte PatternSize;

    public Track(byte frameListSize, byte patternSize)
    {
        Framelist = new byte[frameListSize];
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
        foreach (byte index in Framelist)
        {
            result.Add(Patterns.ElementAt(index));
        }

        return result.ToImmutableList();
    }
}