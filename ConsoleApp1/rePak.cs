namespace ConsoleApp1;

public class RePak
{
    private string InputFolder { get; }
    private string ArchiveSaveName { get; }
    
    private int FileInfoTableOffset = 0x14;
    
    // Input data archive
    private byte[] _hexFile; 
    
    // Old header
    private Header _oldHeader;
    
    // New header
    private Header _newHeader = new Header();
    
    // List of File names from the input archive
    private List<string> _fileNameList;
    
    // Contains File objects
    private List<PakEntryFile> _fileObjectList;

    private byte[] _result;
    
    public RePak(string inputFolder, string inputArchiveFileName, string archiveSaveName)
    {
        InputFolder = inputFolder;
        ArchiveSaveName = archiveSaveName;
        
        // _hexFile = File.ReadAllBytes(inputArchiveFileName);
        _fileNameList = getFilenames();
        
        _oldHeader.UnKnown1 = _hexFile.
    }

    public byte[] BuildArchive()
    {
        BuildNewHeader();

        return null!;
    }

    public void BuildNewHeader()
    {
        _newHeader = new Header();
        _newHeader.UnKnown1 = 0;
        _newHeader.FileCount = 0;
        _newHeader.UnKnown1 = 0;
        _newHeader.UnKnown1 = 0;
        _newHeader.UnKnown1 = 0;
        _newHeader.UnKnown1 = 0;
    }
    
    private List<string> getFilenames()
    {
        throw new NotImplementedException();
    }

}