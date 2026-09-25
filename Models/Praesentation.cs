namespace praesentationsanmeldung.Models;

public class Praesentation
{
    public int Id { get; set; }
    public string Titel { get; set; } = string.Empty;
    public string? Beschreibung { get; set; }
    public DateTime Beginn { get; set; }

    public int RaumId { get; set; }
    public Raum Raum { get; set; } = null!;

    public ICollection<Eintragung> Eintragungen { get; set; } = new List<Eintragung>();
}
