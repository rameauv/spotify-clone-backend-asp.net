﻿namespace SystemTests.Context;

public partial class RefreshToken
{
    public string Id { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string? Data { get; set; }
}
