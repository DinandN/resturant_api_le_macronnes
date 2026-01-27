using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.DTOs;
using LeMacronnesResturauntAPI.Models;
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
        // Ondersteunt: ?id=5 OF ?datum=...&tafelId=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetReserveringen(
            [FromQuery] int? id,
            [FromQuery] DateTime? datum,
            [FromQuery] int? tafelId)
        {
            var query = _context.Reserveringen
                .Include(r => r.Tafel)
                .Include(r => r.Rekening)
                .AsQueryable();

            // 1. Filter op ID
            if (id.HasValue)
            {
                query = query.Where(r => r.ReserveringID == id.Value);
            }

            // 2. Filter op TafelID
            if (tafelId.HasValue)
            {
                query = query.Where(r => r.TafelID == tafelId.Value);
            }

            // 3. Filter op Datum
            if (datum.HasValue)
            {
                query = query.Where(r => r.DatumTijd.Date == datum.Value.Date);
            }

            var result = await query.Select(r => new
            {
                r.ReserveringID,
                r.DatumTijd,
                r.TafelID,
                r.Tafel.Tafelnummer,
                AantalPersonen = r.AantalVolwassenen + r.AantalJongeKinderen + r.AantalOudereKinderen,
                IsGeannuleerd = r.Cancelled,
                RekeningStatus = r.Rekening != null ? r.Rekening.Status : "Geen rekening"
            })
            .ToListAsync();

            if (id.HasValue && !result.Any())
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Reservering>> PostReservering(ReserveringInputDto input)
        {
            DateTime nieuweStart = input.DatumTijd;
            DateTime nieuweEind = input.DatumTijd.AddHours(2);

            bool heeftOverlap = await _context.Reserveringen.AnyAsync(r =>
                r.TafelID == input.TafelID &&
                !r.Cancelled &&
                nieuweStart < r.DatumTijd.AddHours(2) &&
                nieuweEind > r.DatumTijd
            );

            if (heeftOverlap)
            {
                return BadRequest("Deze tafel is niet beschikbaar op dit tijdstip.");
            }

            var reservering = new Reservering
            {
                BoekingID = input.BoekingID,
                DatumTijd = input.DatumTijd,
                AantalVolwassenen = input.AantalVolwassenen,
                AantalJongeKinderen = input.AantalJongeKinderen,
                AantalOudereKinderen = input.AantalOudereKinderen,
                TafelID = input.TafelID,
                Cancelled = false
            };

            _context.Reserveringen.Add(reservering);
            await _context.SaveChangesAsync();

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

        // PUT en DELETE blijven hetzelfde (hieronder ingekort voor overzicht, maar functioneel ongewijzigd)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservering(int id, ReserveringInputDto input)
        {
            var reservering = await _context.Reserveringen.FindAsync(id);
            if (reservering == null) return NotFound();

            reservering.BoekingID = input.BoekingID;
            reservering.DatumTijd = input.DatumTijd;
            reservering.AantalVolwassenen = input.AantalVolwassenen;
            reservering.AantalJongeKinderen = input.AantalJongeKinderen;
            reservering.AantalOudereKinderen = input.AantalOudereKinderen;
            reservering.TafelID = input.TafelID;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservering(int id)
        {
            var reservering = await _context.Reserveringen.FindAsync(id);
            if (reservering == null) return NotFound();

            reservering.Cancelled = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Reservering geannuleerd." });
        }
    }
}