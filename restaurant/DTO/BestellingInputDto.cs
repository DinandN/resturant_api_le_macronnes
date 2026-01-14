namespace LeMacronnesResturauntAPI.DTOs
{
    public class BestellingInputDto
    {
        public int RekeningID { get; set; }
        public List<BestelRegelItemDto> Items { get; set; }
    }

    public class BestelRegelItemDto
    {
        public int GerechtID { get; set; }
        public int Aantal { get; set; }
        public string? Aanpassing { get; set; }
    }
}