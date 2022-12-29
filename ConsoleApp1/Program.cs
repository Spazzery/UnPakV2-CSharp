namespace ConsoleApp1;

class Program
{
    private static Dictionary<string, Action<String>> _commands = new Dictionary<string, Action<string>>()
    {
        ["ls"] = ListEntries,
        ["unpack"] = ExtractContents,
        ["repack"] = RepackContents,
        ["help"] = ShowHelp
    };

    private static string? _path;

    public void Main(string[] args)
    {
        if (args.Length > 0)
        {
            var path = args[0];

            if (path == "ls")
            {
                
            }
        }
        else
        {
            Console.WriteLine("No arguments given!");
        }
    }

    public static void ListEntries()
    {
        throw new NotImplementedException();
    }

    public static void ExtractContents()
    {
        throw new NotImplementedException();
    }

    public static void RepackContents()
    {
        throw new NotImplementedException();
    }

    public static void ShowHelp()
    {
        Console.WriteLine("UnPakV2 - Unpacker/Repacker for CatSystem2 on PS Vita.");
        Console.WriteLine("It's for an updated version of the PAK archive (hence the name).");
        Console.WriteLine("Available commands:");
        Console.WriteLine("ls - Lists input archive's contents in the console");
        Console.WriteLine("unpack - Extracts contents of the input archive");
        Console.WriteLine("repack - Repacks input files into a new archive, based off of the original archive");
        Console.WriteLine("help - Shows this help/info page");
    }
}
