using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;
using GarageApi.Models;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/demandes-vehicule")]
[Authorize]
public class DemandesVehiculeController : ControllerBase
{
    private readonly GarageDbContext _db;

    public DemandesVehiculeController(GarageDbContext db) => _db = db;

    private IQueryable<DemandeVehicule> JoinedQuery() =>
        _db.DemandesVehicule.Include(d => d.Vehicule).Select(d => new DemandeVehicule
        {
            Id = d.Id,
            UserId = d.UserId,
            EmployeNom = d.EmployeNom,
            Service = d.Service,
            VehiculeId = d.VehiculeId,
            Destination = d.Destination,
            Motif = d.Motif,
            DateDemande = d.DateDemande,
            DateDepart = d.DateDepart,
            DateRetourPrevu = d.DateRetourPrevu,
            Statut = d.Statut,
            ChauffeurTraitant = d.ChauffeurTraitant,
            MissionId = d.MissionId,
            DateTraitement = d.DateTraitement,
            CommentaireTraitement = d.CommentaireTraitement,
            DateCreation = d.DateCreation,
            Immatriculation = d.Vehicule != null ? d.Vehicule.Immatriculation : null,
            Marque = d.Vehicule != null ? d.Vehicule.Marque : null,
            Modele = d.Vehicule != null ? d.Vehicule.Modele : null
        });

    private bool IsAdmin() => User.IsInRole("admin");
    private bool IsUser() => User.IsInRole("user");
    private bool IsChauffeur() => User.IsInRole("chauffeur");
    private bool IsMecanicien() => User.IsInRole("mecanicien");

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? statut, [FromQuery] int? vehicule_id)
    {
        var query = JoinedQuery();
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var nomComplet = User.FindFirst("nom_complet")?.Value ?? "";

        if (IsUser())
            query = query.Where(d => d.UserId == userId || d.EmployeNom == nomComplet);
        else if (IsChauffeur())
            query = query.Where(d => d.Statut == "En attente" || d.ChauffeurTraitant == nomComplet);
        else if (!IsAdmin() && !IsMecanicien())
            return StatusCode(403, new { error = "Accès non autorisé" });

        if (!string.IsNullOrEmpty(statut))
            query = query.Where(d => d.Statut == statut);
        if (vehicule_id.HasValue)
            query = query.Where(d => d.VehiculeId == vehicule_id.Value);

        return Ok(query.OrderByDescending(d => d.DateDemande).ToList());
    }

    [HttpPost]
    public IActionResult Create([FromBody] DemandeVehicule demande)
    {
        if (!IsAdmin() && !IsUser())
            return StatusCode(403, new { error = "Accès non autorisé" });

        var nomComplet = User.FindFirst("nom_complet")?.Value ?? "";
        var service = User.FindFirst("service")?.Value ?? "";

        if (IsUser())
        {
            demande.EmployeNom = nomComplet;
            demande.Service = service;
        }

        if (string.IsNullOrWhiteSpace(demande.EmployeNom) || string.IsNullOrWhiteSpace(demande.Service)
            || string.IsNullOrWhiteSpace(demande.Destination) || string.IsNullOrWhiteSpace(demande.DateDepart))
            return BadRequest(new { error = "Employé, service, destination et date départ sont obligatoires" });

        demande.UserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        demande.DateDemande = DateTime.Now.ToString("yyyy-MM-dd");
        demande.DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        demande.Statut = "En attente";

        _db.DemandesVehicule.Add(demande);
        _db.SaveChanges();

        return CreatedAtAction(null, JoinedQuery().FirstOrDefault(d => d.Id == demande.Id));
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] DemandeVehicule updated)
    {
        var existing = _db.DemandesVehicule.Include(d => d.Vehicule).FirstOrDefault(d => d.Id == id);
        if (existing == null)
            return NotFound(new { error = "Demande introuvable" });

        if (IsAdmin())
        {
            existing.UserId = updated.UserId ?? existing.UserId;
            existing.EmployeNom = updated.EmployeNom ?? existing.EmployeNom;
            existing.Service = updated.Service ?? existing.Service;
            existing.VehiculeId = updated.VehiculeId;
            existing.Destination = updated.Destination ?? existing.Destination;
            existing.Motif = updated.Motif ?? existing.Motif;
            existing.DateDepart = updated.DateDepart ?? existing.DateDepart;
            existing.DateRetourPrevu = updated.DateRetourPrevu ?? existing.DateRetourPrevu;
            existing.Statut = updated.Statut ?? existing.Statut;
            existing.ChauffeurTraitant = updated.ChauffeurTraitant ?? existing.ChauffeurTraitant;
            existing.DateTraitement = updated.DateTraitement ?? existing.DateTraitement;
            existing.CommentaireTraitement = updated.CommentaireTraitement ?? existing.CommentaireTraitement;

            if (updated.Statut == "Approuvée" && existing.MissionId == null)
            {
                if (existing.VehiculeId == null)
                    return BadRequest(new { error = "La demande doit être liée à un véhicule pour être approuvée" });

                var vehicule = _db.Vehicules.Find(existing.VehiculeId);
                if (vehicule == null)
                    return BadRequest(new { error = "Véhicule introuvable" });
                if (vehicule.Statut != "Disponible")
                    return BadRequest(new { error = "Le véhicule sélectionné n'est pas disponible" });

                var mission = new Mission
                {
                    VehiculeId = existing.VehiculeId.Value,
                    Chauffeur = existing.ChauffeurTraitant ?? "Chauffeur",
                    Destination = existing.Destination,
                    Motif = existing.Motif,
                    DateDepart = existing.DateDepart,
                    DateRetour = existing.DateRetourPrevu,
                    Statut = "Planifiée",
                    DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                _db.Missions.Add(mission);
                _db.SaveChanges();

                vehicule.Statut = "En mission";
                existing.MissionId = mission.Id;
            }

            _db.SaveChanges();
            return Ok(JoinedQuery().FirstOrDefault(d => d.Id == id));
        }

        if (IsChauffeur())
        {
            if (existing.Statut != "En attente")
                return StatusCode(403, new { error = "Cette demande n'est plus traitable" });

            if (updated.Statut == null || !new[] { "Approuvée", "Refusée" }.Contains(updated.Statut))
                return BadRequest(new { error = "Statut invalide pour un traitement" });

            var nom = User.FindFirst("nom_complet")?.Value ?? "";

            existing.Statut = updated.Statut;
            existing.ChauffeurTraitant = nom;
            existing.DateTraitement = DateTime.Now.ToString("yyyy-MM-dd");
            existing.CommentaireTraitement = updated.CommentaireTraitement;

            if (updated.Statut == "Approuvée")
            {
                if (existing.VehiculeId == null)
                    return BadRequest(new { error = "Impossible d'approuver une demande sans véhicule lié" });

                if (existing.MissionId == null)
                {
                    var vehicule = _db.Vehicules.Find(existing.VehiculeId);
                    if (vehicule == null)
                        return BadRequest(new { error = "Véhicule introuvable" });
                    if (vehicule.Statut != "Disponible")
                        return BadRequest(new { error = "Le véhicule sélectionné n'est pas disponible" });

                    var mission = new Mission
                    {
                        VehiculeId = existing.VehiculeId.Value,
                        Chauffeur = nom,
                        Destination = existing.Destination,
                        Motif = existing.Motif,
                        DateDepart = existing.DateDepart,
                        DateRetour = existing.DateRetourPrevu,
                        Statut = "Planifiée",
                        DateCreation = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                    _db.Missions.Add(mission);
                    _db.SaveChanges();

                    vehicule.Statut = "En mission";
                    existing.MissionId = mission.Id;
                }
            }

            _db.SaveChanges();
            return Ok(JoinedQuery().FirstOrDefault(d => d.Id == id));
        }

        return StatusCode(403, new { error = "Réservé aux administrateurs et chauffeurs" });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (!IsAdmin())
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var demande = _db.DemandesVehicule.Find(id);
        if (demande == null)
            return NotFound(new { error = "Demande introuvable" });

        _db.DemandesVehicule.Remove(demande);
        _db.SaveChanges();
        return NoContent();
    }
}
