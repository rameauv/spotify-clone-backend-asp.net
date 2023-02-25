using Microsoft.AspNetCore.Mvc;

namespace Api.Models.Library;

public class FindLikedTracksQueryParams
{
    [FromQuery(Name = "offset")] public int Offset { get; set; }
    [FromQuery(Name = "limit")] public int Limit { get; set; }
}