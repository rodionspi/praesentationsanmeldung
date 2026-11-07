namespace praesentationsanmeldung.Models;

public class PraesentationMitPlaetzen
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string? Beschreibung { get; set; }
    public DateTime Beginn { get; set; }
    public string Raum { get; set; } = string.Empty;
    public int FreiePlaetze { get; set; }
    public bool Eingetragen { get; set; }
}
