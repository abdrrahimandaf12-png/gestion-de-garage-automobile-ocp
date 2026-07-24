using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApi.Models;

public class Intervention
{
    public int Id { get; set; }

    public int VehiculeId { get; set; }

    [Required]
    public string TypeIntervention { get; set; } = string.Empty;

    [Required]
    public string DateIntervention { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Prestataire { get; set; }

    public double Cout { get; set; } = 0;

    public string Statut { get; set; } = "Planifiée";

    public string? DateProchaineEcheance { get; set; }

    public string DateCreation { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [ForeignKey(nameof(VehiculeId))]
    public Vehicule? Vehicule { get; set; }

    [NotMapped]
    public string? Immatriculation { get; set; }
    [NotMapped]
    public string? Marque { get; set; }
    [NotMapped]
    public string? Modele { get; set; }
}
