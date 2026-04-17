namespace AppServices;

public class Laufkategorie
{
    public int Id { get; set; }
    public string Bezeichnung { get; set; } = string.Empty;
}

public class Laufbewerb
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int LaufkategorieId { get; set; }
    public Laufkategorie? Laufkategorie { get; set; }
    public decimal Streckenlänge { get; set; }
    public DateOnly Datum { get; set; }
    public string Ort { get; set; } = string.Empty;
}

public class Teilnehmer
{
    public int Id { get; set; }
    public int LaufbewerbId { get; set; }
    public Laufbewerb? Laufbewerb { get; set; }
    public int Startnummer { get; set; }
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public int AngestrebteGesamtzeit { get; set; }  // seconds
    public List<Split> Splits { get; set; } = [];
}

public class Split
{
    public int Id { get; set; }
    public int TeilnehmerId { get; set; }
    public Teilnehmer? Teilnehmer { get; set; }
    public int KmNummer { get; set; }
    public int ZeitSekunden { get; set; }
    public decimal SegmentLaenge { get; set; }
}
