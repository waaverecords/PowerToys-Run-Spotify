namespace PowerToys_Run_Spotify;

public class ContextData
{
    public ResultType ResultType { get; set;}
    public string Uri { get; set; }
}

public enum ResultType
{
    Song,
    Artist,
    Playlist
}