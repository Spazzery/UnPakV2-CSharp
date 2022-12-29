using System.Net;

namespace ConsoleApp1;

public class Header
{
    public int UnKnown1 { get; set; } = 0;
    public int FileCount { get; set; } = 0;
    public int VersionNumber { get; set; } = 0;
    public int UnKnown2 { get; set; } = 0;
    public int NameTableOffset { get; set; } = 0;
    public int FirstFileOffset { get; set; } = 0;

    public Header() {}
    
}