using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;
using GarageApi.Models;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/interventions")]
[Authorize]
public class InterventionsController : ControllerBase
{
    private readonly GarageDbContext _db;

    public InterventionsController(GarageDbContext db) => _db = db;

    private bool IsAtelier() => User.IsInRole("admin") || User.IsInRole("mecanicien");

    private IQueryable<Intervention> JoinedQuery() =>
        _db.Interventions.Include(i => i.Vehicule).Select(i => new Intervention
        {
            Id = i.Id,
            VehiculeId = i.VehiculeId,
            TypeIntervention = i.TypeIntervention,
            DateIntervention = i.DateIntervention,
            Description = i.Description,
            Prestataire = i.Prestataire,
            Cout = i.Cout,
            Statut = i.Statut,
            DateProchaineEcheance = i.DateProchaineEcheance,
            DateCreation = i.DateCreation,
            Immatriculation = i.Vehicule != null ? i.Vehicule.Immatriculation : null,
            Marque = i.Vehicule != null ? i.Vehicule.Marque : null,
            Modele = i.Vehicule != null ? i.Vehicule.Modele : null
        });

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? type_intervention, [FromQuery] string? statut, [FromQuery] int? vehicule_id)
    {
        if (!IsAtelier())
            return StatusCode(403, new { error = "Accès réservé à l'atelier" });

        var query = JoinedQuery();

        if (!string.IsNullOrEmpty(type_intervention))
            query = query.Where(i => i.TypeIntervention == type_intervention);
        if (!string.IsNullOrEmpty(statut))
            query = query.Where(i => i.Statut == statut);
        if (vehicule_id.HasValue)
            query = query.Where(i => i.VehiculeId == vehicule_id.Value);

        return Ok(query.OrderByDescending(i => i.DateIntervention).ToList());
    }

    [HttpGet("echeances/proches")]
    public IActionResult GetEcheancesProches()
    {
        if (!IsAtelier())
            return StatusCode(403, new { error = "Accès réservé à l'atelier" });

        var trenteJours = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
        var result = JoinedQuery()
            .Where(i => i.DateProchaineEcheance != null && string.Compare(i.DateProchaineEcheance, trenteJours) <= 0)
            .OrderBy(i => i.DateProchaineEcheance)
            .ToList();

        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Intervention intervention)
    {
        if (!IsAtelier())
            return StatusCode(403, new { error = "Accès réservé à l'atelier" });

        if (intervention.VehiculeId == 0 || string.IsNullOrWhiteSpace(intervention.TypeIntervention)
            || string.IsNullOrWhiteSpace(intervention.DateIntervention))
            return BadRequest(new { error = "Véhicule, type et date sont obligatoires" });

        var vehicule = _db.Vehicules.Find(intervention.VehiculeId);
        if (vehicule == null)
            return BadRequest(new { error = "Véhicule introuvable" });

        intervention.DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        intervention.Statut = string.IsNullOrEmpty(intervention.Statut) ? "Planifiée" : intervention.Statut;

        _db.Interventions.Add(intervention);

        if (intervention.Statut != "Terminée")
            vehicule.Statut = "En réparation";

        _db.SaveChanges();

        return CreatedAtAction(null, JoinedQuery().FirstOrDefault(i => i.Id == intervention.Id));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Intervention updated)
    {
        if (!IsAtelier())
            return StatusCode(403, new { error = "Accès réservé à l'atelier" });

        var existing = _db.Interventions.Include(i => i.Vehicule).FirstOrDefault(i => i.Id == id);
        if (existing == null)
            return NotFound(new { error = "Intervention introuvable" });

        existing.VehiculeId = updated.VehiculeId;
        existing.TypeIntervention = updated.TypeIntervention ?? existing.TypeIntervention;
        existing.DateIntervention = updated.DateIntervention ?? existing.DateIntervention;
        existing.Description = updated.Description;
        existing.Prestataire = updated.Prestataire;
        existing.Cout = updated.Cout;
        existing.Statut = updated.Statut ?? existing.Statut;
        existing.DateProchaineEcheance = updated.DateProchaineEcheance;

        if (updated.Statut == "Terminée")
            existing.Vehicule!.Statut = "Disponible";
        else if (!string.IsNullOrEmpty(updated.Statut))
            existing.Vehicule!.Statut = "En réparation";

        _db.SaveChanges();
        return Ok(JoinedQuery().FirstOrDefault(i => i.Id == id));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!IsAtelier())
            return StatusCode(403, new { error = "Accès réservé à l'atelier" });

        var intervention = _db.Interventions.Find(id);
        if (intervention == null)
            return NotFound(new { error = "Intervention introuvable" });

        _db.Interventions.Remove(intervention);
        _db.SaveChanges();
        return NoContent();
    }
}
