namespace praesentationsanmeldung.Models;

public class Eintragung
{
    public int Id { get; set; }
    public DateTime EingetragenAm { get; set; }

    public int G3SusId { get; set; }
    public G3Sus G3Sus { get; set; } = null!;

    public int PraesentationId { get; set; }
    public Praesentation Praesentation { get; set; } = null!;
}
