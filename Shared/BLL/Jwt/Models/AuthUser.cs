namespace Spotify.Shared.BLL.Jwt.Models;

public record AuthUser(string Id, string UserName)
{
    public string Id { get; set; } = Id;
    
    public string UserName { get; set; } = UserName;

    public string? PasswordHash { get; set; }
}