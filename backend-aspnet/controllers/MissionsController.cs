using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;
using GarageApi.Models;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/missions")]
[Authorize]
public class MissionsController : ControllerBase
{
    private readonly GarageDbContext _db;

    public MissionsController(GarageDbContext db) => _db = db;

    private IQueryable<Mission> JoinedQuery() =>
        _db.Missions.Include(m => m.Vehicule).Select(m => new Mission
        {
            Id = m.Id,
            VehiculeId = m.VehiculeId,
            Chauffeur = m.Chauffeur,
            Destination = m.Destination,
            Motif = m.Motif,
            DateDepart = m.DateDepart,
            DateRetour = m.DateRetour,
            KmDepart = m.KmDepart,
            KmRetour = m.KmRetour,
            Statut = m.Statut,
            DateCreation = m.DateCreation,
            Immatriculation = m.Vehicule != null ? m.Vehicule.Immatriculation : null,
            Marque = m.Vehicule != null ? m.Vehicule.Marque : null,
            Modele = m.Vehicule != null ? m.Vehicule.Modele : null
        });

    private bool IsAdmin() => User.IsInRole("admin");
    private bool IsChauffeur() => User.IsInRole("chauffeur");

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? statut, [FromQuery] int? vehicule_id)
    {
        if (!IsAdmin() && !IsChauffeur())
            return StatusCode(403, new { error = "Accès non autorisé" });

        var query = JoinedQuery();

        if (IsChauffeur())
        {
            var nom = User.FindFirst("nom_complet")?.Value ?? "";
            query = query.Where(m => m.Chauffeur == nom);
        }

        if (!string.IsNullOrEmpty(statut))
            query = query.Where(m => m.Statut == statut);

        if (vehicule_id.HasValue)
            query = query.Where(m => m.VehiculeId == vehicule_id.Value);

        return Ok(query.OrderByDescending(m => m.DateDepart).ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        if (!IsAdmin() && !IsChauffeur())
            return StatusCode(403, new { error = "Accès non autorisé" });

        var query = JoinedQuery().Where(m => m.Id == id);

        if (IsChauffeur())
        {
            var nom = User.FindFirst("nom_complet")?.Value ?? "";
            query = query.Where(m => m.Chauffeur == nom);
        }

        var mission = query.FirstOrDefault();
        if (mission == null)
            return NotFound(new { error = "Mission introuvable" });

        return Ok(mission);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Mission mission)
    {
        if (!IsAdmin())
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        if (mission.VehiculeId == 0 || string.IsNullOrWhiteSpace(mission.Chauffeur)
            || string.IsNullOrWhiteSpace(mission.Destination) || string.IsNullOrWhiteSpace(mission.DateDepart))
            return BadRequest(new { error = "Véhicule, chauffeur, destination et date de départ sont obligatoires" });

        var vehicule = _db.Vehicules.Find(mission.VehiculeId);
        if (vehicule == null)
            return BadRequest(new { error = "Véhicule introuvable" });

        mission.KmDepart ??= vehicule.Kilometrage;
        mission.DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        mission.Statut = string.IsNullOrEmpty(mission.Statut) ? "Planifiée" : mission.Statut;

        _db.Missions.Add(mission);

        if (mission.Statut == "En cours")
            vehicule.Statut = "En mission";

        _db.SaveChanges();

        return CreatedAtAction(nameof(GetById), new { id = mission.Id }, JoinedQuery().FirstOrDefault(m => m.Id == mission.Id));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Mission updated)
    {
        var existing = _db.Missions.Include(m => m.Vehicule).FirstOrDefault(m => m.Id == id);
        if (existing == null)
            return NotFound(new { error = "Mission introuvable" });

        if (IsChauffeur())
        {
            var nom = User.FindFirst("nom_complet")?.Value ?? "";
            if (existing.Chauffeur != nom)
                return StatusCode(403, new { error = "Vous ne pouvez modifier que vos propres missions" });

            if (string.IsNullOrEmpty(updated.Statut) || !new[] { "En cours", "Terminée" }.Contains(updated.Statut))
                return BadRequest(new { error = "Le chauffeur peut uniquement démarrer ou terminer une mission" });

            existing.Statut = updated.Statut;
            if (updated.KmDepart.HasValue) existing.KmDepart = updated.KmDepart;
            if (updated.KmRetour.HasValue) existing.KmRetour = updated.KmRetour;

            if (updated.Statut == "En cours")
                existing.Vehicule!.Statut = "En mission";
            else if (updated.Statut == "Terminée")
            {
                existing.Vehicule!.Statut = "Disponible";
                if (updated.KmRetour.HasValue && updated.KmRetour > existing.Vehicule.Kilometrage)
                    existing.Vehicule.Kilometrage = updated.KmRetour.Value;
            }

            _db.SaveChanges();
            return Ok(JoinedQuery().FirstOrDefault(m => m.Id == id));
        }

        if (!IsAdmin())
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        existing.VehiculeId = updated.VehiculeId;
        existing.Chauffeur = updated.Chauffeur ?? existing.Chauffeur;
        existing.Destination = updated.Destination ?? existing.Destination;
        existing.Motif = updated.Motif ?? existing.Motif;
        existing.DateDepart = updated.DateDepart ?? existing.DateDepart;
        existing.DateRetour = updated.DateRetour ?? existing.DateRetour;
        existing.KmDepart = updated.KmDepart ?? existing.KmDepart;
        existing.KmRetour = updated.KmRetour ?? existing.KmRetour;
        existing.Statut = updated.Statut ?? existing.Statut;

        if (updated.Statut == "Terminée")
        {
            existing.Vehicule!.Statut = "Disponible";
            if (updated.KmRetour.HasValue && updated.KmRetour > existing.Vehicule.Kilometrage)
                existing.Vehicule.Kilometrage = updated.KmRetour.Value;
        }
        else if (updated.Statut == "En cours")
            existing.Vehicule!.Statut = "En mission";

        _db.SaveChanges();
        return Ok(JoinedQuery().FirstOrDefault(m => m.Id == id));
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!IsAdmin())
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var mission = _db.Missions.Find(id);
        if (mission == null)
            return NotFound(new { error = "Mission introuvable" });

        _db.Missions.Remove(mission);
        _db.SaveChanges();
        return NoContent();
    }
}
