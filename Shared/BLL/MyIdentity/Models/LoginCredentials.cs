namespace Spotify.Shared.BLL.MyIdentity.Models;

public record LoginCredentials(string Username, string Password)
{
    public string Username { get; set; } = Username;
    public string Password { get; set; } = Password;
}