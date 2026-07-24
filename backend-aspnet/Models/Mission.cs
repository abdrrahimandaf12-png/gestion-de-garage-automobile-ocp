using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApi.Models;

public class Mission
{
    public int Id { get; set; }

    public int VehiculeId { get; set; }

    [Required]
    public string Chauffeur { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;

    public string? Motif { get; set; }

    [Required]
    public string DateDepart { get; set; } = string.Empty;

    public string? DateRetour { get; set; }

    public int? KmDepart { get; set; }

    public int? KmRetour { get; set; }

    public string Statut { get; set; } = "Planifiée";

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
