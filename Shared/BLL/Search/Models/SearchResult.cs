namespace Spotify.Shared.BLL.Search.Models;

public record SearchResult(
    IEnumerable<ReleaseResult> AlbumResult,
    IEnumerable<SongResult> SongResult,
    IEnumerable<ArtistResult> ArtistResult
)
{
    public IEnumerable<ReleaseResult> AlbumResult { get; set; } = AlbumResult;
    public IEnumerable<SongResult> SongResult { get; set; } = SongResult;
    public IEnumerable<ArtistResult> ArtistResult { get; set; } = ArtistResult;
}

public class BaseResult
{
    public BaseResult(string id, string? thumbnailUrl)
    {
        Id = id;
        ThumbnailUrl = thumbnailUrl;
    }

    public string Id { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Order { get; set; }
}

public class ReleaseResult : BaseResult
{
    public string Title { get; set; }
    public string ArtistName { get; set; }

    public ReleaseResult(
        string id,
        string? thumbnailUrl,
        string title,
        string artistName
    ) : base(id, thumbnailUrl)
    {
        Title = title;
        ArtistName = artistName;
    }
}

public class SongResult : BaseResult
{
    public string Title { get; set; }
    public string ArtistName { get; set; }

    public SongResult(string id, string? thumbnailUrl, string title, string artistName) : base(id, thumbnailUrl)
    {
        Title = title;
        ArtistName = artistName;
    }
}

public class ArtistResult : BaseResult
{
    public string Name { get; set; }

    public ArtistResult(string id, string? thumbnailUrl, string name) : base(id, thumbnailUrl)
    {
        Name = name;
    }
}