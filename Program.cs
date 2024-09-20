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
}