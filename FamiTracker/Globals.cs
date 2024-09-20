namespace FamiTracker;

public class Globals
{
    public byte EngineSpeed { get; set; } = 0;
    public byte FxxSplitPoint { get; set; } = 32;
    public bool LinearPitch { get; set; } = false;
    public Region Machine { get; set; } = Region.NTSC;
    public VibratoStyle VibratoStyle { get; set; } = VibratoStyle.NEW;
    public sbyte SemitonesDetuning { get; set; } = 0;
    public sbyte CentsDetuning { get; set; } = 0;
}