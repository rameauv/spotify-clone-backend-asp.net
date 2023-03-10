using System.Reflection;
using System.Text;
using System.Text.Json;
using Api.AutoMapper;
using Api.Controllers.Shared.Error;
using Api.ExceptionFilters;
using Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealSpotifyDAL;
using RealSpotifyDAL.Repositories;
using RealSpotifyDAL.Repositories.Album;
using RealSpotifyDAL.Repositories.Artist;
using RealSpotifyDAL.Repositories.Track;
using Repositories.Repositories;
using Repositories.Repositories.Like;
using Repositories.Repositories.User;
using Spotify.BLL.Services;
using Spotify.Shared;
using Spotify.Shared.BLL.Album;
using Spotify.Shared.BLL.Artist;
using Spotify.Shared.BLL.Jwt;
using Spotify.Shared.BLL.Library;
using Spotify.Shared.BLL.Like;
using Spotify.Shared.BLL.MyIdentity;
using Spotify.Shared.BLL.Password;
using Spotify.Shared.BLL.Search;
using Spotify.Shared.BLL.Track;
using Spotify.Shared.BLL.User;
using Spotify.Shared.DAL.Album;
using Spotify.Shared.DAL.Artist;
using Spotify.Shared.DAL.IdentityUser;
using Spotify.Shared.DAL.Like;
using Spotify.Shared.DAL.RefreshToken;
using Spotify.Shared.DAL.Search;
using Spotify.Shared.DAL.Track;
using Spotify.Shared.DAL.User;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// CORS
const string debugOrigin = "_debugOrigin";
const string prodOrigin = "_prodOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: debugOrigin,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "capacitor://localhost", "http://localhost");
            policy.AllowCredentials();
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
        });
    options.AddPolicy(name: prodOrigin,
        policy =>
        {
            policy.WithOrigins("capacitor://localhost", "http://localhost");
            policy.AllowCredentials();
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
        });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition(JwtAuthenticationDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = JwtAuthenticationDefaults.HeaderName, // Authorization
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer"
        });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtAuthenticationDefaults.AuthenticationScheme
                }
            },
            new List<string>()
        }
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


var jwtSettingsSection = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwtSettingsSection.GetSection("Issuer").Value;
var jwtAudience = jwtSettingsSection.GetSection("Audience").Value;
var jwtAccessTokenKey = jwtSettingsSection.GetSection("AccessTokenKey").Value;
var jwtRefreshTokenKey = jwtSettingsSection.GetSection("RefreshTokenKey").Value;
var jwtAccessTokenExpiryInMinutes = jwtSettingsSection.GetSection("AccessTokenExpiryInMinutes").Value;
var jwtRefreshTokenExpiryInMinutes = jwtSettingsSection.GetSection("RefreshTokenExpiryInMinutes").Value;
if (jwtIssuer == null
    || jwtAudience == null
    || jwtAccessTokenKey == null
    || jwtRefreshTokenKey == null
    || jwtAccessTokenExpiryInMinutes == null
    || jwtRefreshTokenExpiryInMinutes == null
   )
{
    throw new Exception("the jwt config is missing");
}

var jwtConfig = new JwtConfig(
    jwtIssuer,
    jwtAudience,
    jwtAccessTokenKey,
    jwtRefreshTokenKey,
    double.Parse(jwtAccessTokenExpiryInMinutes),
    double.Parse(jwtRefreshTokenExpiryInMinutes)
);


// Logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// register your examples
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

// AutoMapper
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Project config
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// JWT config
builder.Services.AddSingleton(jwtConfig);

// DAL Dependencies
builder.Services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<MySpotifyClient, MySpotifyClient>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();

// BLL Dependencies
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(jwtConfig.AccessTokenKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
    o.Events = new JwtBearerEvents
    {
        OnChallenge = async (context) =>
        {
            context.HandleResponse();

            var status = StatusCodes.Status401Unauthorized;
            context.Response.StatusCode = status;
            await context.Response.HttpContext.Response.WriteAsJsonAsync(
                new ErrorsDto(new ErrorDto(
                    "unauthorized",
                    status,
                    ""
                )),
                null as JsonSerializerOptions,
                "application/problem+json"
            );
        }
    };
});
builder.Services.AddAuthorization();

builder.Services.AddControllers(options => { options.Filters.Add<GlobalExceptionFilterAttribute>(); });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Tests"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // CORS
    app.UseCors(debugOrigin);
}
else
{
    app.UseCors(prodOrigin);
}


app.UseAuthentication();
app.UseAuthorization();


app.UseHttpsRedirection();

app.MapControllers();

app.Run();

namespace Api
{
    public partial class Program { }
}
