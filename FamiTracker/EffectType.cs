namespace FamiTracker;

public enum EffectType
{
    Arpeggio = '0',
    PitchUp = '1',
    PitchDown = '2',
    AutoPortamento = '3',
    Vibrato = '4',
    Tremolo = '5',
    VolumeSlide = 'A',
    JumpToPattern = 'B',
    Halt = 'C',
    SkipFrameToRow = 'D',
    Volume = 'E',
    Tempo = 'F',
    Delay = 'G',
    HardwareSweepUp = 'H',
    HardwareSweepDown = 'I',
    FDSModDepth = 'H',
    FDSModSpeed = 'I' | 'J',
    FinePitch = 'P',
    NoteSlideUp = 'Q',
    NoteSlideDown = 'R',
    MuteDelay = 'S',
    MiscEffect = 'V',
    DPCMSpeed = 'W',
    DPCMRetrigger = 'X',
    DPCMOffset = 'Y',
    DPCMDelta = 'Z'
}