namespace ConsoleApp1;

public class PakEntryFile
{
    public int Id { get; }
    public string Name { get; }
    public int Offset { get; }
    public int Size { get; }
    // public int RawBinary { get; set; }

    public PakEntryFile(int id, int offset, int size, string name)
    {
        Id = id;
        Offset = id;
        Size = size;
        Name = name;
    }
    
}