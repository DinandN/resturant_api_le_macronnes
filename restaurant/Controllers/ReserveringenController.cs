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
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservering>> GetReservering(int id)
        {
            var reservering = await _context.Reserveringen
                .Include(r => r.Tafel)
                .Include(r => r.Rekening)
                .FirstOrDefaultAsync(r => r.ReserveringID == id);

            if (reservering == null) return NotFound();

            return reservering;
        }

        // PUT: api/Reserveringen/5 (Update standard details)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservering(int id, ReserveringInputDto input)
        {
            var reservering = await _context.Reserveringen.FindAsync(id);
            if (reservering == null) return NotFound();

            // Update allowed fields
            reservering.DatumTijd = input.DatumTijd;
            reservering.AantalVolwassenen = input.AantalVolwassenen;
            reservering.AantalJongeKinderen = input.AantalJongeKinderen;
            reservering.AantalOudereKinderen = input.AantalOudereKinderen;
            reservering.TafelID = input.TafelID;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Reserveringen/5
        // IMPLEMENTATION: Soft Delete (Mark as Cancelled)
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservering(int id)
        {
            var reservering = await _context.Reserveringen.FindAsync(id);
            if (reservering == null) return NotFound();

            // Instead of .Remove, we change the status
            reservering.Cancelled = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservering is geannuleerd (niet verwijderd)." });
        }
    }
}