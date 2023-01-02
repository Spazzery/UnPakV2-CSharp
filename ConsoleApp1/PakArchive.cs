using System.Text;

namespace ConsoleApp1;

public class PakArchive
{
    private Stream _fileStream;
    private BinaryReader _fileReader;

    public List<PakEntryFile>? Entries { get; set; }
    private Func<int, PakEntryFile>? ReadEntryFunc;  // contains a function that is used to create PakEntryFile objects depending on mode
    
    public List<string> _nameTable = new List<string>();  // contains entry names
    
    private int _fileInfoTableOffset = 0x14;

    // In the order of the header (first 5)
    private Int32 _unKnown1;
    private Int32 _entryCount;
    private Int32 _versionNumber;
    private Int32 _unKnown2;
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
        
        FileStream fileStream = File.Open(path, FileMode.Open);
        
        return new PakArchive(fileStream);
    }
    
    // Actually doesn't just "read" entries, but also creates PakEntryFiles in the process
    private void ReadEntries()
    {
        Entries = new List<PakEntryFile>();
        _unKnown1 = _fileReader.ReadInt32(); // already reads in little endian
        _entryCount = _fileReader.ReadInt32();
        _versionNumber = _fileReader.ReadInt32();
        _unKnown2 = _fileReader.ReadInt32();
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
        using FileStream outFile = new FileStream(Path.ChangeExtension(outPath, "") + "\\" + entry.Name, FileMode.Create);
        using BinaryWriter bw = new BinaryWriter(outFile);
        bw.Write(data);
        Console.WriteLine($"Extracted file: {entry.Name}");
    }

    private byte[] ExtractEntry(PakEntryFile entry)
    {
        _fileStream.Seek(entry.Offset, SeekOrigin.Begin);
        return _fileReader.ReadBytes(entry.Size);
    }


    public static void Repack(string inPath, string outPath)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _fileReader.Dispose();
        _fileStream.Close();
    }
    

}