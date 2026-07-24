using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApi.Models;

public class Consommation
{
    public int Id { get; set; }

    public int VehiculeId { get; set; }

    [Required]
    public string TypeConso { get; set; } = "Carburant";

    [Required]
    public string DateConso { get; set; } = string.Empty;

    public double Quantite { get; set; }

    public string Unite { get; set; } = "L";

    public double CoutUnitaire { get; set; } = 0;

    [NotMapped]
    public double CoutTotal => Quantite * CoutUnitaire;

    public int? Kilometrage { get; set; }

    public string? Fournisseur { get; set; }

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
