using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace praesentationsanmeldung.Models;

public class Praesentation
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Bitte einen Titel eingeben.")]
    [StringLength(200, ErrorMessage = "Der Titel darf maximal 200 Zeichen sein.")]
    public string Titel { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Der Titel darf maximal 2000 Zeichen sein.")]
    public string? Beschreibung { get; set; }
    public DateTime Beginn { get; set; }

    public int RaumId { get; set; }

    [ValidateNever]
    public Raum Raum { get; set; } = null!;

    public ICollection<Eintragung> Eintragungen { get; set; } = new List<Eintragung>();
}
