namespace praesentationsanmeldung.Models;

public class Raum
{
    public int Id { get; set; }
    public string Bezeichnung { get; set; } = string.Empty;
    public int Kapazitaet { get; set; }

    public ICollection<Praesentation> Praesentationen { get; set; } = new List<Praesentation>();
}
