namespace LeMacronnesResturauntAPI.DTOs
{
    public class ReserveringInputDto
    {
        public DateTime DatumTijd { get; set; }
        public byte AantalVolwassenen { get; set; }
        public byte AantalJongeKinderen { get; set; }
        public byte AantalOudereKinderen { get; set; }
        public int TafelID { get; set; }
    }
}