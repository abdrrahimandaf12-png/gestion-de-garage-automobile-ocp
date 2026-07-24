using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;
using GarageApi.Models;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly GarageDbContext _db;

    public UsersController(GarageDbContext db) => _db = db;

    [HttpGet]
    public IActionResult GetAll()
    {
        var users = _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.NomComplet,
                u.Service,
                u.Actif,
                u.DateCreation
            })
            .OrderBy(u => u.Id)
            .ToList();

        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public IActionResult GetById(int id)
    {
        var user = _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.NomComplet,
                u.Service,
                u.Actif,
                u.DateCreation
            })
            .FirstOrDefault(u => u.Id == id);

        if (user == null)
            return NotFound(new { error = "Utilisateur introuvable" });

        return Ok(user);
    }

    [HttpPost]
    public IActionResult Create([FromBody] UserDto request)
    {
        var username = request.Username?.Trim();
        var role = request.Role?.Trim();
        var nomComplet = request.NomComplet?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(nomComplet))
            return BadRequest(new { error = "username, password et nom_complet sont requis" });

        role ??= "";

        if (_db.Users.Any(u => u.Username == username))
            return Conflict(new { error = "Cet identifiant est déjà utilisé" });

        var user = new User
        {
            Username = username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            NomComplet = nomComplet,
            Service = string.IsNullOrWhiteSpace(request.Service) ? null : request.Service.Trim(),
            Actif = request.Actif,
            DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new
        {
            user.Id,
            user.Username,
            user.Role,
            user.NomComplet,
            user.Service,
            user.Actif,
            user.DateCreation
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public IActionResult Update(int id, [FromBody] UserDto request)
    {
        var existing = _db.Users.Find(id);
        if (existing == null)
            return NotFound(new { error = "Utilisateur introuvable" });

        var username = string.IsNullOrWhiteSpace(request.Username) ? existing.Username : request.Username.Trim();
        var role = request.Role != null ? request.Role.Trim() : existing.Role;
        var nomComplet = string.IsNullOrWhiteSpace(request.NomComplet) ? existing.NomComplet : request.NomComplet.Trim();

        if (username != existing.Username && _db.Users.Any(u => u.Username == username))
            return Conflict(new { error = "Cet identifiant est déjà utilisé" });

        existing.Username = username;
        existing.Role = role;
        existing.NomComplet = nomComplet;
        existing.Service = request.Service != null ? (string.IsNullOrWhiteSpace(request.Service) ? null : request.Service.Trim()) : existing.Service;
        existing.Actif = request.Actif;

        if (!string.IsNullOrEmpty(request.Password))
            existing.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        _db.SaveChanges();

        return Ok(new
        {
            existing.Id,
            existing.Username,
            existing.Role,
            existing.NomComplet,
            existing.Service,
            existing.Actif,
            existing.DateCreation
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public IActionResult Delete(int id)
    {
        var existing = _db.Users.Find(id);
        if (existing == null)
            return NotFound(new { error = "Utilisateur introuvable" });

        existing.Actif = 0;
        _db.SaveChanges();
        return NoContent();
    }
}

public class UserDto
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? NomComplet { get; set; }
    public string? Service { get; set; }
    public int Actif { get; set; } = 1;
}
