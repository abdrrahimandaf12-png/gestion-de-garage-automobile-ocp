using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApi.Models;

public class DemandeVehicule
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required]
    public string EmployeNom { get; set; } = string.Empty;

    [Required]
    public string Service { get; set; } = string.Empty;

    public int? VehiculeId { get; set; }

    [Required]
    public string Destination { get; set; } = string.Empty;

    public string? Motif { get; set; }

    public string DateDemande { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

    [Required]
    public string DateDepart { get; set; } = string.Empty;

    public string? DateRetourPrevu { get; set; }

    public string Statut { get; set; } = "En attente";

    public string? ChauffeurTraitant { get; set; }

    public int? MissionId { get; set; }

    public string? DateTraitement { get; set; }

    public string? CommentaireTraitement { get; set; }

    public string DateCreation { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [ForeignKey(nameof(VehiculeId))]
    public Vehicule? Vehicule { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [NotMapped]
    public string? Immatriculation { get; set; }
    [NotMapped]
    public string? Marque { get; set; }
    [NotMapped]
    public string? Modele { get; set; }
}
