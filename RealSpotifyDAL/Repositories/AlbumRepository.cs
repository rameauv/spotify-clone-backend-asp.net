using RealSpotifyDAL.Repositories.Album.Extensions;
using Spotify.Shared.DAL.Album;
using Spotify.Shared.DAL.Album.Models;
using SpotifyAPI.Web;
using RealSpotify = SpotifyAPI.Web;
using SharedDAL = Spotify.Shared.DAL;

namespace RealSpotifyDAL.Repositories;

/// <summary>
/// Repository for fetching album information from the Spotify API
/// </summary>
public class AlbumRepository : IAlbumRepository
{
    private readonly SpotifyClient _spotifyClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumRepository"/> class.
    /// </summary>
    /// <param name="spotifyClient">Spotify client object</param>
    public AlbumRepository(MySpotifyClient spotifyClient)
    {
        this._spotifyClient = spotifyClient.SpotifyClient;
    }

    public async Task<Spotify.Shared.DAL.Album.Models.Album?> GetAsync(string id)
    {
        try
        {
            var resAlbum = await _spotifyClient.Albums.Get(id, new AlbumRequest());
            var artistId = resAlbum.Artists.FirstOrDefault()?.Id;
            if (artistId == null)
            {
                throw new Exception("could not get the artist id for this album");
            }

            var resArtist = await _spotifyClient.Artists.Get(artistId);
            return resAlbum.ToAlbum();
        }
        catch (APIException e)
        {
            if (e.Message == "invalid id")
            {
                return null;
            }

            throw;
        }
    }

    public async Task<AlbumTracks?> GetTracksAsync(string id,
        SharedDAL.Album.Models.AlbumTracksRequest? albumTracksRequest = null)
    {
        try
        {
            var res = await _spotifyClient.Albums.GetTracks(id, new RealSpotify.AlbumTracksRequest()
            {
                Limit = albumTracksRequest?.Limit,
                Offset = albumTracksRequest?.Offset
            });
            if (res.Items == null)
            {
                return new AlbumTracks(Array.Empty<SharedDAL.Album.Models.SimpleTrack>());
            }

            var mappedTracks = res.Items.Select(track => new SharedDAL.Album.Models.SimpleTrack(
                track.Id,
                track.Name,
                track.Artists.FirstOrDefault()?.Name ?? "")
            ).ToArray();
            return new AlbumTracks(mappedTracks, res.Limit ?? 0, res.Offset ?? 0, res.Total ?? 0);
        }
        catch (APIException e)
        {
            if (e.Message == "invalid id")
            {
                return null;
            }

            throw;
        }
    }

    public async Task<IEnumerable<SharedDAL.Album.Models.Album>> GetAlbums(IEnumerable<string> albumIds)
    {
        var res = await _spotifyClient.Albums.GetSeveral(new AlbumsRequest(albumIds.ToList()));
        return res.Albums.Select(album => album.ToAlbum());
    }
}