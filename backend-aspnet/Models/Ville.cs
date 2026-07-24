using System.ComponentModel.DataAnnotations;

namespace GarageApi.Models;

public class Ville
{
    public int Id { get; set; }

    [Required]
    public string Nom { get; set; } = string.Empty;
}
