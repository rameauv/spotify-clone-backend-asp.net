using System.Net.Mime;
using Api.Controllers.Album.Models;
using Api.Controllers.Search.Models;
using Api.Controllers.Shared;
using Api.Controllers.Shared.Error;
using Api.Controllers.Shared.Like;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotify.Shared.BLL.Album;
using Spotify.Shared.BLL.Album.Models;
using Spotify.Shared.BLL.Jwt;

namespace Api.Controllers.Album;

/// <summary>
/// Controller for handling album-related requests
/// </summary>
[Route("[controller]")]
[Authorize]
[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json, "application/problem+json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorsDto))]
[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorsDto))]
public class AlbumController : MyControllerBase
{
    private readonly IAlbumService _albumService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlbumController"/> class.
    /// </summary>
    /// <param name="albumService">The album service.</param>
    /// <param name="jwtService">The jwt service.</param>
    public AlbumController(IAlbumService albumService, IJwtService jwtService) : base(jwtService)
    {
        this._albumService = albumService;
    }

    /// <summary>
    /// Get the album by its id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlbumDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorsDto))]
    public async Task<IActionResult> Get(string id)
    {
        var userId = GetCurrentUserId();
        var res = await _albumService.GetAsync(id, userId);

        if (res == null)
        {
            return Error(new ErrorDto(
                "bad request",
                StatusCodes.Status400BadRequest,
                "invalid id"
            ));
        }

        var result = new AlbumDto(
            res.Id,
            res.Title,
            res.ReleaseDate,
            res.ThumbnailUrl,
            res.ArtistId,
            res.ArtistName,
            res.ArtistThumbnailUrl,
            res.AlbumType,
            res.LikeId
        );
        return Ok(result);
    }

    /// <summary>
    /// Get the associated tracks of an album
    /// </summary>
    [HttpGet("{id}/Tracks")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlbumTracksDto))]
    public async Task<IActionResult> Tracks(string id, int? limit, int? offset)
    {
        var res = await _albumService.GetTracksAsync(id, new AlbumTracksRequest
        {
            Limit = limit,
            Offset = offset
        });
        if (res == null)
        {
            return Error(new ErrorDto(
                "bad request",
                StatusCodes.Status400BadRequest,
                "invalid id"
            ));
        }

        var mappedTracks = res.Items.Select(track => new AlbumTrackDto(
            track.Id,
            track.Title,
            track.ArtistName
        )).ToArray();
        var result = new AlbumTracksDto(
            mappedTracks,
            res.Limit,
            res.Offset,
            res.Total
        );
        return Ok(result);
    }


    /// <summary>
    /// Set the like status for the album
    /// </summary>
    [HttpPatch("{id}/Like")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LikeDto))]
    public async Task<IActionResult> SetLike(string id)
    {
        var userId = GetCurrentUserId();

        var like = await _albumService.SetLikeAsync(id, userId);

        return Ok(new LikeDto(like.Id));
    }
}