using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApi.Models;

public class Vehicule
{
    public int Id { get; set; }

    [Required]
    public string Immatriculation { get; set; } = string.Empty;

    [Required]
    public string Marque { get; set; } = string.Empty;

    [Required]
    public string Modele { get; set; } = string.Empty;

    [Required]
    public string TypeVehicule { get; set; } = "Léger";

    public string? DateAcquisition { get; set; }

    public int Kilometrage { get; set; } = 0;

    public string? ServiceAffecte { get; set; }

    public string Statut { get; set; } = "Disponible";

    public string DateCreation { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public ICollection<Mission> Missions { get; set; } = new List<Mission>();
    public ICollection<Consommation> Consommations { get; set; } = new List<Consommation>();
    public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    public ICollection<DemandeVehicule> DemandesVehicule { get; set; } = new List<DemandeVehicule>();
}
