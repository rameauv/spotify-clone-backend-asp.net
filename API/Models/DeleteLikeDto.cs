using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class DeleteLikeDto
{
    public DeleteLikeDto(string id)
    {
        Id = id;
    }

    public string Id { get; }
}