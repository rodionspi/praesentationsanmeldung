namespace praesentationsanmeldung.Models;

public class G3Sus
{
    public int Id { get; set; }
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public string Klasse { get; set; } = string.Empty;

    public ICollection<Eintragung> Eintragungen { get; set; } = new List<Eintragung>();
}
