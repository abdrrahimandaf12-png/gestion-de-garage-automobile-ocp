using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using GarageApi.Data;
using GarageApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly GarageDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(GarageDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Identifiant et mot de passe requis" });

        var user = _db.Users.FirstOrDefault(u => u.Username == request.Username && u.Actif == 1);
        if (user == null)
            return Unauthorized(new { error = "Identifiant ou mot de passe incorrect" });

        bool passwordMatches;
        if (user.Password.StartsWith("$2"))
            passwordMatches = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        else
            passwordMatches = user.Password == request.Password;

        if (!passwordMatches)
            return Unauthorized(new { error = "Identifiant ou mot de passe incorrect" });

        if (!user.Password.StartsWith("$2"))
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            _db.SaveChanges();
        }

        var token = CreateToken(user);
        return Ok(new
        {
            token,
            user = new
            {
                user.Id,
                user.Username,
                user.Role,
                user.NomComplet,
                user.Service,
                user.Actif
            }
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.NomComplet))
            return BadRequest(new { error = "Nom, identifiant et mot de passe requis" });

        if (request.Password.Length < 6)
            return BadRequest(new { error = "Le mot de passe doit contenir au moins 6 caractères" });

        if (_db.Users.Any(u => u.Username == request.Username))
            return Conflict(new { error = "Cet identifiant est déjà utilisé" });

        var user = new User
        {
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "user",
            NomComplet = request.NomComplet,
            Service = request.Service ?? "Service",
            Actif = 1
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        var token = CreateToken(user);
        return CreatedAtAction(null, new
        {
            token,
            user = new
            {
                user.Id,
                user.Username,
                user.Role,
                user.NomComplet,
                user.Service,
                user.Actif
            }
        });
    }

    private string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "garage_phosboucraa_secret_key_256bits_min!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("nom_complet", user.NomComplet ?? ""),
            new Claim("service", user.Service ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "GarageApi",
            audience: _config["Jwt:Audience"] ?? "GarageApi",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string? Service { get; set; }
}
