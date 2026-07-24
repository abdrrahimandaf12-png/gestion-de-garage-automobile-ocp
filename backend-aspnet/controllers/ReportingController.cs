using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GarageApi.Data;

namespace GarageApi.Controllers;

[ApiController]
[Route("api/reporting")]
[Authorize]
public class ReportingController : ControllerBase
{
    private readonly GarageDbContext _db;

    public ReportingController(GarageDbContext db) => _db = db;

    [HttpGet("kpis")]
    public IActionResult GetKpis()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var nomComplet = User.FindFirst("nom_complet")?.Value ?? "";
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var now = DateTime.Now;
        var debutMois = new DateTime(now.Year, now.Month, 1).ToString("yyyy-MM-dd");
        var trenteJours = now.AddDays(30).ToString("yyyy-MM-dd");

        if (role == "admin")
        {
            var totalVehicules = _db.Vehicules.Count();
            var vehiculesParStatut = _db.Vehicules.GroupBy(v => v.Statut).Select(g => new { statut = g.Key, n = g.Count() }).ToList();
            var missionsEnCours = _db.Missions.Count(m => m.Statut == "En cours");
            var missionsMois = _db.Missions.Count(m => string.Compare(m.DateDepart, debutMois) >= 0);
            var coutCarburantMois = _db.Consommations.Where(c => c.TypeConso == "Carburant" && string.Compare(c.DateConso, debutMois) >= 0).Sum(c => c.Quantite * c.CoutUnitaire);
            var coutLubrifiantMois = _db.Consommations.Where(c => c.TypeConso == "Lubrifiant" && string.Compare(c.DateConso, debutMois) >= 0).Sum(c => c.Quantite * c.CoutUnitaire);
            var coutInterventionsMois = _db.Interventions.Where(i => string.Compare(i.DateIntervention, debutMois) >= 0).Sum(i => i.Cout);
            var echeancesProches = _db.Interventions.Count(i => i.DateProchaineEcheance != null && string.Compare(i.DateProchaineEcheance, trenteJours) <= 0);

            return Ok(new
            {
                totalVehicules,
                vehiculesParStatut,
                missionsEnCours,
                missionsMois,
                coutCarburantMois,
                coutLubrifiantMois,
                coutInterventionsMois,
                coutTotalMois = coutCarburantMois + coutLubrifiantMois + coutInterventionsMois,
                echeancesProches
            });
        }

        if (role == "mecanicien")
        {
            var totalVehicules = _db.Vehicules.Count();
            var enReparation = _db.Vehicules.Count(v => v.Statut == "En réparation");
            var interventionsEnCours = _db.Interventions.Count(i => i.Statut == "En cours");
            var interventionsMois = _db.Interventions.Count(i => string.Compare(i.DateIntervention, debutMois) >= 0);
            var echeancesProches = _db.Interventions.Count(i => i.DateProchaineEcheance != null && string.Compare(i.DateProchaineEcheance, trenteJours) <= 0);
            var coutInterventionsMois = _db.Interventions.Where(i => string.Compare(i.DateIntervention, debutMois) >= 0).Sum(i => i.Cout);
            var vehiculesParStatut = _db.Vehicules.GroupBy(v => v.Statut).Select(g => new { statut = g.Key, n = g.Count() }).ToList();

            return Ok(new
            {
                totalVehicules,
                enReparation,
                interventionsEnCours,
                interventionsMois,
                coutInterventionsMois,
                echeancesProches,
                vehiculesParStatut
            });
        }

        if (role == "chauffeur")
        {
            var missionsEnCours = _db.Missions.Count(m => m.Chauffeur == nomComplet && m.Statut == "En cours");
            var missionsMois = _db.Missions.Count(m => m.Chauffeur == nomComplet && string.Compare(m.DateDepart, debutMois) >= 0);
            var vehiculesDisponibles = _db.Vehicules.Count(v => v.Statut == "Disponible");
            var totalVehicules = _db.Vehicules.Count();
            var vehiculesParStatut = _db.Vehicules.GroupBy(v => v.Statut).Select(g => new { statut = g.Key, n = g.Count() }).ToList();

            return Ok(new
            {
                missionsEnCours,
                missionsMois,
                vehiculesDisponibles,
                totalVehicules,
                vehiculesParStatut
            });
        }

        // user role
        {
            var vehiculesDisponibles = _db.Vehicules.Count(v => v.Statut == "Disponible");
            var totalVehicules = _db.Vehicules.Count();
            var vehiculesParStatut = _db.Vehicules.GroupBy(v => v.Statut).Select(g => new { statut = g.Key, n = g.Count() }).ToList();
            var mesDemandes = _db.DemandesVehicule.Count(d => d.UserId == userId || d.EmployeNom == nomComplet);
            var demandesApprouvees = _db.DemandesVehicule.Count(d => (d.UserId == userId || d.EmployeNom == nomComplet) && d.Statut == "Approuvée");

            return Ok(new
            {
                vehiculesDisponibles,
                totalVehicules,
                vehiculesParStatut,
                mesDemandes,
                demandesApprouvees
            });
        }
    }

    [HttpGet("couts-par-vehicule")]
    public IActionResult CoutsParVehicule()
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var result = _db.Vehicules
            .Select(v => new
            {
                v.Id,
                v.Immatriculation,
                v.Marque,
                v.Modele,
                v.TypeVehicule,
                totalConsommations = _db.Consommations.Where(c => c.VehiculeId == v.Id).Sum(c => c.Quantite * c.CoutUnitaire),
                totalInterventions = _db.Interventions.Where(i => i.VehiculeId == v.Id).Sum(i => i.Cout)
            })
            .ToList()
            .Select(x => new
            {
                x.Id,
                x.Immatriculation,
                x.Marque,
                x.Modele,
                x.TypeVehicule,
                totalConsommations = x.totalConsommations,
                totalInterventions = x.totalInterventions,
                totalGeneral = x.totalConsommations + x.totalInterventions
            })
            .OrderByDescending(x => x.totalGeneral)
            .ToList();

        return Ok(result);
    }

    [HttpGet("consommation-mensuelle")]
    public IActionResult ConsommationMensuelle()
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var result = _db.Consommations
            .GroupBy(c => new { mois = c.DateConso.Substring(0, 7), c.TypeConso })
            .Select(g => new
            {
                mois = g.Key.mois,
                typeConso = g.Key.TypeConso,
                quantiteTotale = g.Sum(c => c.Quantite),
                coutTotal = g.Sum(c => c.Quantite * c.CoutUnitaire)
            })
            .OrderBy(x => x.mois)
            .ToList();

        return Ok(result);
    }

    [HttpGet("missions-par-statut")]
    public IActionResult MissionsParStatut()
    {
        if (!User.IsInRole("admin"))
            return StatusCode(403, new { error = "Réservé à l'administrateur" });

        var result = _db.Missions.GroupBy(m => m.Statut)
            .Select(g => new { statut = g.Key, n = g.Count() })
            .ToList();

        return Ok(result);
    }

    [HttpGet("villes-stats")]
    public IActionResult VillesStats()
    {
        var villes = _db.Villes.ToList();

        var missionsParDest = _db.Missions
            .GroupBy(m => m.Destination)
            .Select(g => new { destination = g.Key, total = g.Count(), enCours = g.Count(m => m.Statut == "En cours") })
            .ToDictionary(x => x.destination);

        var demandesParDest = _db.DemandesVehicule
            .GroupBy(d => d.Destination)
            .Select(g => new { destination = g.Key, total = g.Count() })
            .ToDictionary(x => x.destination);

        var stats = villes.Select(v =>
        {
            missionsParDest.TryGetValue(v.Nom, out var m);
            demandesParDest.TryGetValue(v.Nom, out var d);
            return new
            {
                nom = v.Nom,
                missions = m?.total ?? 0,
                missionsEnCours = m?.enCours ?? 0,
                demandes = d?.total ?? 0
            };
        }).ToList();

        return Ok(new { villes = stats });
    }

    [HttpGet("interventions-par-type")]
    public IActionResult InterventionsParType()
    {
        if (!User.IsInRole("admin") && !User.IsInRole("mecanicien"))
            return StatusCode(403, new { error = "Accès non autorisé" });

        var result = _db.Interventions.GroupBy(i => i.TypeIntervention)
            .Select(g => new { typeIntervention = g.Key, n = g.Count(), coutTotal = g.Sum(i => i.Cout) })
            .ToList();

        return Ok(result);
    }
}
