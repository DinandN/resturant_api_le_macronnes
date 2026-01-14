using System.ComponentModel.DataAnnotations;

namespace LeMacronnesResturauntAPI.Models;

public class Reservering
{
    [Key]
    public int ReserveringID { get; set; }
    public int? BoekingID { get; set; }
    public DateTime DatumTijd { get; set; }
    public bool Cancelled { get; set; }
    public byte AantalVolwassenen { get; set; }
    public byte AantalJongeKinderen { get; set; }
    public byte AantalOudereKinderen { get; set; }

    public int TafelID { get; set; }
    public Tafel Tafel { get; set; }

    public Rekening? Rekening { get; set; }
}