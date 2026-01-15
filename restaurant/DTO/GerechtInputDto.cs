namespace LeMacronnesResturauntAPI.DTOs
{
    public class GerechtInputDto
    {
        public string Naam { get; set; } = string.Empty;
        public string Omschrijving { get; set; } = string.Empty;
        public decimal Prijs { get; set; }
        public string Allergenen { get; set; } = string.Empty;
    }
}