using System.ComponentModel.DataAnnotations;

namespace praesentationsanmeldung.Models;

public class Raum
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Bitte eine Bezeichnung eingeben.")]
    [StringLength(50, ErrorMessage = "Die Bezeichnung darf maximal 50 Zeichen lang sein.")]
    public string Bezeichnung { get; set; } = "";

    [Range(1, 1000, ErrorMessage = "Bitte eine Zahl von 1 bis 1000 eingeben.")]
    public int Kapazitaet { get; set; }
    public ICollection<Praesentation> Praesentationen { get; set; } = new List<Praesentation>();
    // ICollection macht eine Sammlung von Pr�sentation-Objekten. Eigentlich bedeutet das, dass ein Raum
    // hat viele Pr�sentationen, jedi Pr�sentation findet in genau einem Raum statt.

    // {get; set;} bedeutet, dass man den Wert von Eigenschaften lesen(get) und setzen(set) kann.

    // Bezeichnung, Kapazitaet, Id sind Namen der Eigenschaften und sp�ter auch Namen der Spalten in der DB.
}
