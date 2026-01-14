using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeMacronnesResturauntAPI.Models;

public class Gerecht
{
    [Key]
    public int GerechtID { get; set; }

    [MaxLength(250)]
    public string Naam { get; set; } = string.Empty;

    public string Omschrijving { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Prijs { get; set; }

    public string Allergenen { get; set; } = string.Empty;
}