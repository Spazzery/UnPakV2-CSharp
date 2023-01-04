namespace ConsoleApp1;

class Program
{
    private static Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>
    {
        ["ls"] = ListEntries,
        ["unpack"] = ExtractContents,
        ["repack"] = RepackContents,
        ["help"] = ShowHelp,
        ["exit"] = Exit
    };

    private static PakArchive? _archive;

    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0] == "ls")
            {
                _commands["ls"].Invoke(args);
            }
            
            else if (args[0] == "unpack")
            {
                _commands["unpack"].Invoke(args);
            }

            else if (args[0] == "repack")
            {
                _commands["repack"].Invoke(args);
            }
            
            else if (args[0] == "help")
            {
                _commands["help"].Invoke(args);
            }

            else
            {
                Console.WriteLine("No such command. Try command 'help' for more info.");
            }
        }
        else
        {
            // Console.WriteLine("No arguments given!");
            _commands["help"].Invoke(args);
            InteractiveMode();
            Console.WriteLine("Not enough arguments. Please specify the input folder or file.");
        }
    }

    private static void InteractiveMode()
    {
        string inputs;
        do
        {
            inputs = Console.ReadLine()!;
            var args = inputs.Split(' ');
            
            if (_commands.ContainsKey(args[0]))  // command
                _commands[args[0]].Invoke(args);
            
            else 
                Console.WriteLine("Unknown command!");

        } while (!inputs.Equals("exit", StringComparison.OrdinalIgnoreCase));
    }

    public static void ListEntries(string[] args)
    {
        if (args.Length == 2)
        {
            // var command = args[0];
            string inputPak = args[1];
            _archive = PakArchive.LoadArchive(inputPak);
            _archive.ListEntries();
        } 
        else
        {
            Console.WriteLine("Invalid arguments!");
        }
    }

    public static void ExtractContents(string[] args)
    {
        if (args.Length is 2 or 3)  // args.Length == 2 || args.Length == 3
        {
            string inFile = args[1];
            string outPath = args.Length == 3 ? args[2] : "out";  // ternary for choosing an outpath or using a default outpath
            
            if (!Directory.Exists(Path.ChangeExtension(outPath, "")))
                Directory.CreateDirectory(Path.ChangeExtension(outPath, ""));
            
            _archive = PakArchive.LoadArchive(inFile);
            _archive.Unpack(outPath);
        }
        else
        {
            Console.WriteLine("Invalid arguments!");
        }
    }

    public static void RepackContents(string[] args)
    {
        if (args.Length is 3 or 4)
        {   
            string inFolder = args[1];
            string inPak = args[2];
            string newFileName = args.Length == 4 ? args[3] : "NewPak.PAK";  // if filename isn't given

            if (!File.GetAttributes(inFolder).HasFlag(FileAttributes.Directory)) // if inputFolder is actually a file
                throw new ArgumentException("Input folder must be a folder, not a file.");

            if (!newFileName.Contains(".PAK"))
                newFileName += ".PAK";
            
            _archive = PakArchive.LoadArchive(inPak);  // load in the original PAK file we want to copy some info from
            _archive.CreateNew(inFolder, newFileName);
        }
        else
        {
            Console.WriteLine("Invalid arguments!");
        }
    }

    public static void Exit(string[] args)
    {
        _archive!.Dispose();
    } 

    public static void ShowHelp(string[] args)
    {
        Console.WriteLine("----------");
        Console.WriteLine("UnPakV2 - Unpacker/Repacker for CatSystem2 on PS Vita.");
        Console.WriteLine("It's for an updated version of the PAK archive (hence the name).");
        Console.WriteLine("Available commands:");
        Console.WriteLine("ls - Lists input archive's contents in the console. Usage: 'ls <input Pak file>");
        Console.WriteLine("unpack - Extracts contents of the input archive. Usage: 'unpack <input Pak file> [output path]'");
        Console.WriteLine("repack - Creates a new Pak archive, based off of the original archive. Usage: 'repack <input folder> <input Pak to copy from> [new archive name]'");
        Console.WriteLine("help - Shows this help/info page");
        Console.WriteLine("----------");
    }
}
