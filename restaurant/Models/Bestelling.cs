using System.ComponentModel.DataAnnotations;

namespace LeMacronnesResturauntAPI.Models;

public class Bestelling
{
    [Key]
    public int BestellingID { get; set; }

    public int RekeningID { get; set; }
    public Rekening Rekening { get; set; }

    public ICollection<BestelRegel> BestelRegels { get; set; }
}