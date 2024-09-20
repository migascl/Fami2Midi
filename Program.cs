using System.Text.Json;
using FamiTracker;

namespace Fami2Midi;

internal class Program
{
    static void Main(string[] args)
    {
        string content = ReadFile(args[0]);
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
                    Pattern pattern = track.AddPattern(patternEl.GetProperty("index").GetByte());

                    // Notes
                    JsonElement eventsEl = patternEl.GetProperty("notes");
                    foreach (JsonElement eventEl in eventsEl.EnumerateArray())
                    {
                        JsonElement noteEl = eventEl.GetProperty("note");

                        Event note = new Event();
                        if (Enum.TryParse(noteEl.GetProperty("kind").GetString(), true, out NoteType kind))
                            note.Kind = kind;
                        if (noteEl.TryGetProperty("volume", out JsonElement volume)) note.Volume = volume.GetByte();
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
}