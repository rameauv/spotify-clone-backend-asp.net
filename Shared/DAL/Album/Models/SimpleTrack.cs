namespace Spotify.Shared.DAL.Album.Models;

public record SimpleTrack(string Id, string Title, string ArtistName)
{
    public string Id { get; set; } = Id;
    public string Title { get; set; } = Title;
    public string ArtistName { get; set; } = ArtistName;
}