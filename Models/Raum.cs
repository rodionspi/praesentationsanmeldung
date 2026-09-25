namespace praesentationsanmeldung.Models;

public class Raum
{
    public int Id { get; set; }
    public string Bezeichnung { get; set; } = "";
    public int Kapazitaet { get; set; }
    public ICollection<Praesentation> Praesentationen { get; set; } = new List<Praesentation>();
    // ICollection macht eine Sammlung von Präsentation-Objekten. Eigentlich bedeutet das, dass ein Raum
    // hat viele Präsentationen, jedi Präsentation findet in genau einem Raum statt.

    // {get; set;} bedeutet, dass man den Wert von Eigenschaften lesen(get) und setzen(set) kann.

    // Bezeichnung, Kapazitaet, Id sind Namen der Eigenschaften und später auch Namen der Spalten in der DB.
}
