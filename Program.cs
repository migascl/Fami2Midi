using System.Text.Json;
using System.Collections.Immutable;
using FamiTracker;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace Fami2Midi;

internal class Program
{
    static void Main(string[] args)
    {
        string jsonString = ReadFile(args[0]);
        Project project = ParseFile(jsonString);

        MidiFile midiFile = SongToMidi(project, 0);
        midiFile.Write(String.Format("{0} - {1} ({2}).mid", project.Metadata.Artist, project.Metadata.Title, 0), true);

        Console.WriteLine("Done.");
    }

    static string ReadFile(string path)
    {
        FileInfo file = new FileInfo(path);
        if (!file.Exists) throw new FileNotFoundException();
        if (file.Extension != ".json") throw new FormatException("Not a JSON file.");
        return File.ReadAllText(path);
    }

    private static Project ParseFile(string jsonString)
    {
        Project project = new Project();

        var options = new JsonSerializerOptions
        {
            WriteIndented = true, // Pretty-print JSON
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Use camelCase naming
        };

        JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement rootEl = doc.RootElement;

        // Global settings
        JsonElement globalEl = rootEl.GetProperty("global");
        project.Globals = new Globals
        {
            EngineSpeed = globalEl.GetProperty("engine_speed").GetByte(),
            FxxSplitPoint = globalEl.GetProperty("fxx_split_point").GetByte(),
            LinearPitch = globalEl.GetProperty("linear_pitch").GetBoolean(),
            Machine = Enum.Parse<Region>(globalEl.GetProperty("machine").GetString(), true),
            VibratoStyle = Enum.Parse<VibratoStyle>(globalEl.GetProperty("vibrato_style").GetString(), true),
            SemitonesDetuning = globalEl.GetProperty("detune").GetProperty("semitones").GetSByte(),
            CentsDetuning = globalEl.GetProperty("detune").GetProperty("cents").GetSByte(),
        };

        // Metadata
        JsonElement metadataEl = rootEl.GetProperty("metadata");
        project.Metadata = new Metadata
        {
            Artist = metadataEl.GetProperty("artist").GetString(),
            Comment = metadataEl.GetProperty("comment").GetString(),
            Copyright = metadataEl.GetProperty("copyright").GetString(),
            Title = metadataEl.GetProperty("title").GetString()
        };

        // Instruments
        JsonElement instrumentsEl = rootEl.GetProperty("instruments");
        foreach (JsonElement instrumentEl in instrumentsEl.EnumerateArray())
        {
            project.Instruments.Add(new Instrument
            {
                Chip = Enum.Parse<Chip>(instrumentEl.GetProperty("chip").GetString().Insert(0, "_"), true),
                Name = instrumentEl.GetProperty("name").GetString()
            });
        }

        // Songs
        JsonElement songsEl = rootEl.GetProperty("songs");
        foreach (JsonElement songEl in songsEl.EnumerateArray())
        {
            Song song = new Song
            {
                Frames = songEl.GetProperty("frames").GetByte(),
                Rows = songEl.GetProperty("rows").GetByte(),
                Speed = songEl.GetProperty("speed").GetByte(),
                Tempo = songEl.GetProperty("tempo").GetByte(),
                Title = songEl.GetProperty("title").GetString(),
            };

            // Tracks
            JsonElement tracksEl = songEl.GetProperty("tracks");
            foreach (JsonElement trackEl in tracksEl.EnumerateArray())
            {
                Track track = new Track(song.Frames, song.Rows);
                track.Framelist = JsonSerializer.Deserialize<List<byte>>(trackEl.GetProperty("frame_list").ToString())
                    .ToArray();

                // Patterns
                JsonElement patternsEl = trackEl.GetProperty("patterns");
                foreach (JsonElement patternEl in patternsEl.EnumerateArray())
                {
                    FamiTracker.Pattern pattern = track.AddPattern(patternEl.GetProperty("index").GetByte());

                    // Notes
                    JsonElement eventsEl = patternEl.GetProperty("notes");
                    foreach (JsonElement eventEl in eventsEl.EnumerateArray())
                    {
                        JsonElement noteEl = eventEl.GetProperty("note");

                        Event note = new Event { Id = eventEl.GetProperty("row").GetByte() };
                        if (Enum.TryParse(noteEl.GetProperty("kind").GetString(), true, out NoteType kind))
                            note.Kind = kind;
                        if (noteEl.TryGetProperty("volume", out JsonElement volume)) note.Volume = (byte)((byte.MaxValue * volume.GetByte()) / 15);
                        if (noteEl.TryGetProperty("inst_index", out JsonElement instIndex))
                            note.Instrument = project.Instruments.ElementAt(instIndex.GetByte());
                        if (noteEl.TryGetProperty("value", out JsonElement noteValue)) note.Value = noteValue.GetByte();

                        // Effects
                        if (noteEl.TryGetProperty("effects", out JsonElement effects))
                        {
                            foreach (JsonElement effectEl in effects.EnumerateArray())
                            {
                                note.Effects.Add(new Effect
                                {
                                    Type = (EffectType)effectEl.GetProperty("name").GetString().ElementAt(0),
                                    Value = effectEl.GetProperty("param").GetByte()
                                });
                            }
                        }

                        pattern.SetEvent(eventEl.GetProperty("row").GetByte(), note);
                    }
                }

                song.Tracks.Add(track);
            }

            project.Songs.Add(song);
        }

        return project;
    }

    private static MidiFile SongToMidi(Project project, byte songIndex)
    {
        MidiFile midiFile = new MidiFile();
        Song song = project.Songs.ElementAt(songIndex);
        TempoMap tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(song.Tempo));
        midiFile.ReplaceTempoMap(tempoMap);

        for (int trackId = 0; trackId < song.Tracks.Count; trackId++)
        {
            PatternBuilder patternBuilder = new PatternBuilder();
            patternBuilder.SetStep(MusicalTimeSpan.Sixteenth);
            patternBuilder.SetNoteLength(MusicalTimeSpan.Sixteenth);

            Track track = song.Tracks.ElementAt(trackId);
            track.GetPatternSequence().ForEach((FamiTracker.Pattern pattern) =>
            {
                patternBuilder.MoveToTime((song.Rows * pattern.Id) * MusicalTimeSpan.Sixteenth);
                patternBuilder.Anchor();
                // For debugging
                //patternBuilder.Marker(pattern.Id.ToString());

                ImmutableList<Event> _notesList = pattern.Events.Where(e => e != null).ToImmutableList();
                for (int i = 0; i < _notesList.Count; i++)
                {
                    Event currentEvent = _notesList.ElementAt(i);

                    patternBuilder.MoveToLastAnchor();
                    patternBuilder.StepForward(currentEvent.Id * MusicalTimeSpan.Sixteenth);

                    // Get next row index
                    Event? nextEvent = _notesList.ElementAtOrDefault(i + 1);
                    int nextRow = nextEvent != null ? nextEvent.Id : song.Rows - 1;

                    if (currentEvent.Kind == NoteType.Note)
                    {
                        Note note = Note.Get((SevenBitNumber)(currentEvent.Value ?? 0));
                        patternBuilder.Note(note, (nextRow - currentEvent.Id) * MusicalTimeSpan.Sixteenth);
                    }

                    if (currentEvent.Volume != null)
                    {
                        SevenBitNumber volume = (SevenBitNumber)((byte)SevenBitNumber.MaxValue * currentEvent.Volume / byte.MaxValue);
                        patternBuilder.ControlChange(ControlName.ChannelVolume.AsSevenBitNumber(), volume);
                    }

                    // TODO: Add effects support
                }
            });

            TrackChunk trackChunk = patternBuilder.Build().ToTrackChunk(tempoMap, (FourBitNumber)trackId);
            midiFile.Chunks.Add(trackChunk);
        }

        return midiFile;
    }
}