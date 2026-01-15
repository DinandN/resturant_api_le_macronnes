namespace LeMacronnesResturauntAPI.DTOs
{
    public class RekeningBetalingDto
    {
        public decimal TotaalBetaald { get; set; }
        public string BetaalMethode { get; set; } = "Onbekend";
    }
}