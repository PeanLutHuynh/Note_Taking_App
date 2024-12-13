using System.Collections.Generic;

public class NoteEntry
{
    public string Title { get; set; }
    public string Message { get; set; }
    public List<string> ImagePaths { get; set; } = new List<string>(); 
    public List<string> MusicPaths { get; set; } = new List<string>(); 

    public NoteEntry(string title, string message)
    {
        Title = title;
        Message = message;
    }
}
