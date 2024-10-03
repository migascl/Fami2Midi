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

    public Pattern AddPattern(byte? index)
    {
        byte id = index ?? Convert.ToByte(_Patterns.Count);
        if (_Patterns.Find(p => p.Id == id) == null)
        {
            Pattern pattern = new Pattern(id, PatternSize);
            _Patterns.Add(pattern);
            return pattern;
        }
        else
        {
            throw new Exception("Pattern does not exist");
        }
    }

    public ImmutableList<Pattern> GetPatternSequence()
    {
        List<Pattern> result = new List<Pattern>();
        foreach (byte index in Framelist)
        {
            Pattern? pattern = Patterns.ElementAtOrDefault(index);
            if(pattern != null)
            {
                result.Add(Patterns.ElementAt(index));
            } else
            {
                continue;
            }
            
        }

        return result.ToImmutableList();
    }
}