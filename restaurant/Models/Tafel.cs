using System.ComponentModel.DataAnnotations;

namespace LeMacronnesResturauntAPI.Models;

public class Tafel
{
    [Key]
    public int TafelID { get; set; }
    public int Tafelnummer { get; set; }
    public int AantalPlaatsen { get; set; }

    public ICollection<Reservering> Reserveringen { get; set; }
}