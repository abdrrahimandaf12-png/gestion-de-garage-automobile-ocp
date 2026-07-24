using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GarageApi.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "";

    [Required]
    public string NomComplet { get; set; } = string.Empty;

    public string? Service { get; set; }

    public int Actif { get; set; } = 1;

    public string DateCreation { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [NotMapped]
    public string? Token { get; set; }
}
