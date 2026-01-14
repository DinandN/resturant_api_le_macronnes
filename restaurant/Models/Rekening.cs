using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeMacronnesResturauntAPI.Models;

public class Rekening
{
    [Key]
    public int RekeningID { get; set; }

    [MaxLength(50)]
    public string BetaalMethode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Open";

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotaalBetaald { get; set; }

    public int ReserveringID { get; set; }
    public Reservering Reservering { get; set; }

    public ICollection<Bestelling> Bestellingen { get; set; }
}