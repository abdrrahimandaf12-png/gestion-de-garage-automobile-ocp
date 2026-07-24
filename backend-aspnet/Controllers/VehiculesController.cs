using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;
using GarageApi.Models;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/vehicules")]
[Authorize]
public class VehiculesController : ControllerBase
{
    private readonly GarageDbContext _db;

    public VehiculesController(GarageDbContext db) => _db = db;

    private bool IsAdmin() => User.IsInRole("admin");
    private bool IsChauffeur() => User.IsInRole("chauffeur");
    private bool IsUser() => User.IsInRole("user");

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? statut, [FromQuery] string? type)
    {
        var query = _db.Vehicules.AsQueryable();

        if (IsUser())
            query = query.Where(v => v.Statut == "Disponible");

        if (!string.IsNullOrEmpty(statut))
            query = query.Where(v => v.Statut == statut);

        if (!string.IsNullOrEmpty(type))
            query = query.Where(v => v.TypeVehicule == type);

        var result = query.OrderBy(v => v.Marque).ThenBy(v => v.Modele).ToList();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var vehicule = _db.Vehicules.Find(id);
        if (vehicule == null)
            return NotFound(new { error = "Véhicule introuvable" });

        if (IsUser() && vehicule.Statut != "Disponible")
            return StatusCode(403, new { error = "Accès réservé aux véhicules disponibles" });

        return Ok(vehicule);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Vehicule vehicule)
    {
        if (!IsAdmin())
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        if (string.IsNullOrWhiteSpace(vehicule.Immatriculation) || string.IsNullOrWhiteSpace(vehicule.Marque)
            || string.IsNullOrWhiteSpace(vehicule.Modele) || string.IsNullOrWhiteSpace(vehicule.TypeVehicule))
            return BadRequest(new { error = "Immatriculation, marque, modèle et type sont obligatoires" });

        try
        {
            vehicule.DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            vehicule.Statut = string.IsNullOrEmpty(vehicule.Statut) ? "Disponible" : vehicule.Statut;
            _db.Vehicules.Add(vehicule);
            _db.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = vehicule.Id }, vehicule);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
        {
            return BadRequest(new { error = "Cette immatriculation existe déjà" });
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Vehicule updated)
    {
        var existing = _db.Vehicules.Find(id);
        if (existing == null)
            return NotFound(new { error = "Véhicule introuvable" });

        if (IsAdmin())
        {
            existing.Immatriculation = updated.Immatriculation ?? existing.Immatriculation;
            existing.Marque = updated.Marque ?? existing.Marque;
            existing.Modele = updated.Modele ?? existing.Modele;
            existing.TypeVehicule = updated.TypeVehicule ?? existing.TypeVehicule;
            existing.DateAcquisition = updated.DateAcquisition ?? existing.DateAcquisition;
            existing.Kilometrage = updated.Kilometrage;
            existing.ServiceAffecte = updated.ServiceAffecte;
            existing.Statut = updated.Statut ?? existing.Statut;

            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                return BadRequest(new { error = "Cette immatriculation existe déjà" });
            }
            return Ok(existing);
        }

        if (IsChauffeur())
        {
            if (updated.Statut == null || !new[] { "Disponible", "En réparation" }.Contains(updated.Statut))
                return StatusCode(403, new { error = "Le chauffeur peut uniquement mettre le véhicule en Disponible ou En réparation" });

            existing.Statut = updated.Statut;
            _db.SaveChanges();
            return Ok(existing);
        }

        return StatusCode(403, new { error = "Réservé aux administrateurs et chauffeurs" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!IsAdmin())
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var vehicule = _db.Vehicules.Find(id);
        if (vehicule == null)
            return NotFound(new { error = "Véhicule introuvable" });

        _db.Vehicules.Remove(vehicule);
        _db.SaveChanges();
        return NoContent();
    }
}
