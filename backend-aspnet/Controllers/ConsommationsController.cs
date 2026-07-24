using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;
using GarageApi.Models;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/consommations")]
[Authorize]
public class ConsommationsController : ControllerBase
{
    private readonly GarageDbContext _db;

    public ConsommationsController(GarageDbContext db) => _db = db;

    private IQueryable<Consommation> JoinedQuery() =>
        _db.Consommations.Include(c => c.Vehicule).Select(c => new Consommation
        {
            Id = c.Id,
            VehiculeId = c.VehiculeId,
            TypeConso = c.TypeConso,
            DateConso = c.DateConso,
            Quantite = c.Quantite,
            Unite = c.Unite,
            CoutUnitaire = c.CoutUnitaire,
            Kilometrage = c.Kilometrage,
            Fournisseur = c.Fournisseur,
            DateCreation = c.DateCreation,
            Immatriculation = c.Vehicule != null ? c.Vehicule.Immatriculation : null,
            Marque = c.Vehicule != null ? c.Vehicule.Marque : null,
            Modele = c.Vehicule != null ? c.Vehicule.Modele : null
        });

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? type_conso, [FromQuery] int? vehicule_id, [FromQuery] string? du, [FromQuery] string? au)
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var query = JoinedQuery();

        if (!string.IsNullOrEmpty(type_conso))
            query = query.Where(c => c.TypeConso == type_conso);
        if (vehicule_id.HasValue)
            query = query.Where(c => c.VehiculeId == vehicule_id.Value);
        if (!string.IsNullOrEmpty(du))
            query = query.Where(c => string.Compare(c.DateConso, du) >= 0);
        if (!string.IsNullOrEmpty(au))
            query = query.Where(c => string.Compare(c.DateConso, au) <= 0);

        return Ok(query.OrderByDescending(c => c.DateConso).ToList());
    }

    [HttpPost]
    public IActionResult Create([FromBody] Consommation conso)
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        if (conso.VehiculeId == 0 || string.IsNullOrWhiteSpace(conso.TypeConso) || string.IsNullOrWhiteSpace(conso.DateConso))
            return BadRequest(new { error = "Véhicule, type, date et quantité sont obligatoires" });

        conso.Unite = string.IsNullOrEmpty(conso.Unite) ? "L" : conso.Unite;
        conso.DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        _db.Consommations.Add(conso);
        _db.SaveChanges();

        return CreatedAtAction(null, JoinedQuery().FirstOrDefault(c => c.Id == conso.Id));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Consommation updated)
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var existing = _db.Consommations.Find(id);
        if (existing == null)
            return NotFound(new { error = "Enregistrement introuvable" });

        existing.VehiculeId = updated.VehiculeId;
        existing.TypeConso = updated.TypeConso ?? existing.TypeConso;
        existing.DateConso = updated.DateConso ?? existing.DateConso;
        existing.Quantite = updated.Quantite;
        existing.Unite = updated.Unite ?? existing.Unite;
        existing.CoutUnitaire = updated.CoutUnitaire;
        existing.Kilometrage = updated.Kilometrage;
        existing.Fournisseur = updated.Fournisseur;

        _db.SaveChanges();
        return Ok(JoinedQuery().FirstOrDefault(c => c.Id == id));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var conso = _db.Consommations.Find(id);
        if (conso == null)
            return NotFound(new { error = "Enregistrement introuvable" });

        _db.Consommations.Remove(conso);
        _db.SaveChanges();
        return NoContent();
    }
}
