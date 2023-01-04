using System.Text;

namespace ConsoleApp1;

public class PakArchive
{
    private Stream _fileStream;
    private BinaryReader _fileReader;

    private List<PakEntryFile>? Entries { get; set; }
    private List<PakEntryFile>? NewEntries { get; set; }
    private Func<int, PakEntryFile>? ReadEntryFunc;  // contains a function that is used to create PakEntryFile objects depending on mode
    
    private List<string> _nameTable = new List<string>();  // contains entry names
    
    private int _fileInfoTableOffset = 0x14;

    // In the order of the header (first 5)
    private Int32 _firstFileOffset;
    private Int32 _entryCount;
    private Int32 _versionNumber;
    private Int32 _unKnown1;
    private Int32 _nameTableOffset;

    public PakArchive(Stream stream)
    {
        _fileStream = stream;
        _fileReader = new BinaryReader(stream, Encoding.UTF8, true);
        ReadEntries();
    }
    
    public static PakArchive LoadArchive(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));
        
        FileStream fileStream = File.Open(path, FileMode.Open);  // note to self: don't use 'using' here - will make it unable to be read later
        
        return new PakArchive(fileStream);
    }
    
    // Actually doesn't just "read" entries, but also creates PakEntryFiles in the process
    private void ReadEntries()
    {
        Entries = new List<PakEntryFile>();
        _firstFileOffset = _fileReader.ReadInt32(); // already reads in little endian
        _entryCount = _fileReader.ReadInt32();
        _versionNumber = _fileReader.ReadInt32();
        _unKnown1 = _fileReader.ReadInt32();
        _nameTableOffset = _fileReader.ReadInt32();

        ReadEntryFunc = ReadEntryName;  // having this gives option to have different functions for different modes (In unPAK there's 2 modes: ID or Name)
        
        // First, read NameTable and create a list of names
        _fileStream.Seek(_nameTableOffset, SeekOrigin.Begin);  // set offset to read from
        ReadNameTable();
        
        // Then, read each file's info from File Info Table and create PakEntryFile objects
        _fileStream.Seek(_fileInfoTableOffset, SeekOrigin.Begin);
        for (int i = 0; i < _entryCount; i++)
        {
            Entries.Add(ReadEntryFunc(i));  // currently there's only 1 mode, so it's basically just doing CreateEntryFile(i)
        }
    }

    private void ReadNameTable()
    {
        List<byte> entryNameAsBytes = new List<byte>();
        
        while (_nameTable.Count != _entryCount)
        {  
            // Read null terminated strings
            byte character = _fileReader.ReadByte();  // advance byte by byte

            if (character != 0x0) {
                entryNameAsBytes.Add(character);
            }
            else {
                _nameTable.Add(Encoding.UTF8.GetString(entryNameAsBytes.ToArray()));
                entryNameAsBytes = new List<byte>();
            }
        }
    }

    public void ListEntries()
    {
        foreach (PakEntryFile entry in Entries!)
        {
            double size = Convert.ToDouble(entry.Size / 1024);
            Console.WriteLine($"{entry.Name} File size: {size}KB");
        }
    }

    private PakEntryFile ReadEntryName(int id)
    {
        int offset = _fileReader.ReadInt32();
        int size = _fileReader.ReadInt32();
        return new PakEntryFile(id, offset, size, _nameTable[id]);
    }

    public void Unpack(string outPath)
    {
        foreach (PakEntryFile entry in Entries!)
        {
            byte[] data = ExtractEntry(entry);
            WriteToFile(entry, data, outPath);
        }
        Console.WriteLine($"Finished extracting all {Entries.Count} files from the archive!");
        Dispose();
    }

    private void WriteToFile(PakEntryFile entry, byte[] data, string outPath)
    {
        using FileStream outFile = new FileStream(Path.ChangeExtension(outPath, "") + Path.DirectorySeparatorChar + entry.Name, FileMode.Create);
        using BinaryWriter bw = new BinaryWriter(outFile);
        bw.Write(data);
        Console.WriteLine($"Extracted file: {entry.Name}");
    }

    private byte[] ExtractEntry(PakEntryFile entry)
    {
        _fileStream.Seek(entry.Offset, SeekOrigin.Begin);
        return _fileReader.ReadBytes(entry.Size);
    }
    
    public void CreateNew(string inFolder, string newFileName)
    {
        using FileStream pakFile = new FileStream(newFileName, FileMode.Create);
        using BinaryWriter bw = new BinaryWriter(pakFile);
        string[] inputFiles = GetInputFilesInCorrectOrderFrom(Directory.GetFiles(inFolder, "*", SearchOption.TopDirectoryOnly));  // sorts based off input archive
        
        if (inputFiles.Length != _entryCount)
            throw new Exception("Input folder contains less/more files than in the original archive.");
        
        // Add header values to pakFile
        bw.Write(_firstFileOffset);  // copied from input archive, as all other header values
        bw.Write(_entryCount);
        bw.Write(_versionNumber);
        bw.Write(_unKnown1);
        bw.Write(_nameTableOffset);  // Could also use 0x14 + _entryCount * 8;
        
        // Calculating offsets for input files
        NewEntries = new List<PakEntryFile>();
        int fileOffset = _firstFileOffset;
        
        for (int i = 0; i < _entryCount; i++)
        {
            string filename = inputFiles[i];  // not merely a name, but a path
            byte[] rawBinary = File.ReadAllBytes(filename);

            PakEntryFile entry = new PakEntryFile(i, fileOffset, rawBinary.Length, filename);
            entry.RawBinary = rawBinary;
            NewEntries.Add(entry);
            fileOffset += rawBinary.Length;
        }
        
        // Add file info to pakFile
        foreach (PakEntryFile entry in NewEntries!)
        {
            bw.Write(entry.Offset);
            bw.Write(entry.Size);
        }
        
        // Add nameTable to pakFile
        byte[] nameTable = CopyBytes(_nameTableOffset, _firstFileOffset); // copied from input archive
        bw.Write(nameTable);
        
        // Add raw binary to pakFile
        foreach (PakEntryFile entry in NewEntries!)
        {
            Console.WriteLine($"Packing file: {Path.GetFileName(entry.Name)}");
            bw.Write(entry.RawBinary!);
        }

        Console.WriteLine("Repack finished!");
    }

    private byte[] CopyBytes(int from, int to)
    {
        int size = to - from;
        _fileStream.Seek(from, SeekOrigin.Begin);
        return _fileReader.ReadBytes(size);
    }
    
    private string[] GetInputFilesInCorrectOrderFrom(string[] inputFiles)
    {
        List<string> result = new List<string>();
        
        foreach (string inputArchiveEntryName in _nameTable)
        {
            foreach (string inputFile in inputFiles)
            {
                if (inputArchiveEntryName == Path.GetFileName(inputFile))  // inputFile is a path technically
                    result.Add(inputFile);
            }
        }
        
        return result.ToArray();
    }

    public void Dispose()
    {
        _fileReader.Dispose();
        _fileStream.Close();
    }
    

}