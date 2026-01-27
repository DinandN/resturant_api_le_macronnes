namespace LeMacronnesResturauntAPI.DTOs
{
    public class RekeningOutputDto
    {
        public int RekeningID { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotaalBetaald { get; set; }

        // Kept as int (Strict) per your request
        public int TafelID { get; set; }
        public int BoekingID { get; set; }

        public List<RekeningItemDto> Items { get; set; } = new();
    }

    public class RekeningItemDto
    {
        public string GerechtNaam { get; set; } = string.Empty;
        public int Aantal { get; set; }
        public decimal PrijsPerStuk { get; set; }
        public decimal RegelTotaal => Aantal * PrijsPerStuk;
    }
}