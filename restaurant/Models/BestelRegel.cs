using System.ComponentModel.DataAnnotations;

namespace LeMacronnesResturauntAPI.Models;

public class BestelRegel
{
    [Key]
    public int BestelRegelID { get; set; }

    public int Aantal { get; set; }
    public string? Aanpassing { get; set; }

    public int BestellingID { get; set; }
    public Bestelling Bestelling { get; set; }

    public int GerechtID { get; set; }
    public Gerecht Gerecht { get; set; }
}