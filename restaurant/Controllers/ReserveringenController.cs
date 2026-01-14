using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.Models;
using LeMacronnesResturauntAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMacronnesResturauntAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReserveringenController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public ReserveringenController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/Reserveringen
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetReserveringen()
        {
            // LINQ: Haal reserveringen op + tafelnummer + rekeningstatus
            var query = await _context.Reserveringen
                .Include(r => r.Tafel)
                .Include(r => r.Rekening)
                .Select(r => new
                {
                    r.ReserveringID,
                    r.DatumTijd,
                    r.Tafel.Tafelnummer,
                    TotaalPersonen = r.AantalVolwassenen + r.AantalJongeKinderen + r.AantalOudereKinderen,
                    RekeningStatus = r.Rekening != null ? r.Rekening.Status : "Geen rekening"
                })
                .ToListAsync();

            return Ok(query);
        }

        // POST: api/Reserveringen
        [HttpPost]
        public async Task<ActionResult<Reservering>> PostReservering(ReserveringInputDto input)
        {
            // 1. Maak Reservering aan
            var reservering = new Reservering
            {
                DatumTijd = input.DatumTijd,
                AantalVolwassenen = input.AantalVolwassenen,
                AantalJongeKinderen = input.AantalJongeKinderen,
                AantalOudereKinderen = input.AantalOudereKinderen,
                TafelID = input.TafelID,
                Cancelled = false
            };

            _context.Reserveringen.Add(reservering);
            await _context.SaveChangesAsync();

            // 2. Maak automatisch een lege rekening aan
            var rekening = new Rekening
            {
                ReserveringID = reservering.ReserveringID,
                Status = "Open",
                TotaalBetaald = 0,
                BetaalMethode = "Onbekend"
            };

            _context.Rekeningen.Add(rekening);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserveringen), new { id = reservering.ReserveringID }, reservering);
        }
    }
}