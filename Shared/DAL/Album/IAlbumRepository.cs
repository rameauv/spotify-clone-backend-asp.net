using Spotify.Shared.DAL.Album.Models;

namespace Spotify.Shared.DAL.Album;

/// <summary>
/// Repository for fetching album information
/// </summary>
public interface IAlbumRepository
{
    /// <summary>
    /// Retrieves an album by its ID.
    /// </summary>
    /// <param name="id">The ID of the album.</param>
    /// <returns>The album with the specified ID, or null if no such album exists.</returns>
    public Task<Models.Album?> GetAsync(string id);
    /// <summary>
    /// Retrieves a list of tracks for a given album.
    /// </summary>
    /// <param name="id">The ID of the album to retrieve tracks for.</param>
    /// <param name="albumTracksRequest">A request object for specifying pagination options for the track list. If null, default values will be used.</param>
    /// <returns>An object containing the retrieved tracks and track list metadata, or null if no such album exists.</returns>
    public Task<AlbumTracks?> GetTracksAsync(string id, AlbumTracksRequest? albumTracksRequest = null);

    public Task<IEnumerable<Models.Album>> GetAlbumsAsync(IEnumerable<string> albumIds);
}