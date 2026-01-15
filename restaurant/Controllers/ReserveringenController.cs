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
        // Ondersteunt optionele filters: ?datum=2026-01-15&tafelId=3
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetReserveringen(
            [FromQuery] DateTime? datum,
            [FromQuery] int? tafelId)
        {
            var query = _context.Reserveringen
                .Include(r => r.Tafel)
                .Include(r => r.Rekening)
                .AsQueryable();

            // Filter op TafelID als die is ingevuld
            if (tafelId.HasValue)
            {
                query = query.Where(r => r.TafelID == tafelId.Value);
            }

            // Filter op Datum (alleen de dag, tijd negeren we voor de match)
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

            return Ok(result);
        }

        // GET: api/Reserveringen/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservering>> GetReservering(int id)
        {
            var reservering = await _context.Reserveringen
                .Include(r => r.Tafel)
                .Include(r => r.Rekening)
                .FirstOrDefaultAsync(r => r.ReserveringID == id);

            if (reservering == null)
            {
                return NotFound();
            }

            return reservering;
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
                return BadRequest("Deze tafel is niet beschikbaar op dit tijdstip. Er moet minimaal 2 uur tussen reserveringen zitten.");
            }


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

            var rekening = new Rekening
            {
                ReserveringID = reservering.ReserveringID,
                Status = "Open",
                TotaalBetaald = 0,
                BetaalMethode = "Onbekend"
            };

            _context.Rekeningen.Add(rekening);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservering), new { id = reservering.ReserveringID }, reservering);
        }

        // PUT: api/Reserveringen/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservering(int id, ReserveringInputDto input)
        {
            var reservering = await _context.Reserveringen.FindAsync(id);
            if (reservering == null)
            {
                return NotFound();
            }

            reservering.DatumTijd = input.DatumTijd;
            reservering.AantalVolwassenen = input.AantalVolwassenen;
            reservering.AantalJongeKinderen = input.AantalJongeKinderen;
            reservering.AantalOudereKinderen = input.AantalOudereKinderen;
            reservering.TafelID = input.TafelID;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Reserveringen/5
        // Dit is een SOFT DELETE (Annuleren)
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservering(int id)
        {
            var reservering = await _context.Reserveringen.FindAsync(id);
            if (reservering == null)
            {
                return NotFound();
            }

            // We verwijderen hem niet echt, maar zetten hem op cancelled
            reservering.Cancelled = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservering geannuleerd (Soft Delete)." });
        }
    }
}