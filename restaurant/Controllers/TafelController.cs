using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.DTOs;
using LeMacronnesResturauntAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMacronnesResturauntAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TafelsController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public TafelsController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/Tafels
        // Optioneel: ?aantalPlaatsen=4&beschikbaarOp=2026-01-15T18:00:00
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tafel>>> GetTafels(
            [FromQuery] int? aantalPlaatsen,
            [FromQuery] DateTime? beschikbaarOp)
        {
            var query = _context.Tafels.AsQueryable();

            // Filter 1: Minimaal aantal plaatsen
            if (aantalPlaatsen.HasValue)
            {
                query = query.Where(t => t.AantalPlaatsen >= aantalPlaatsen.Value);
            }

            // Filter 2: Beschikbaarheid op een datum/tijd
            if (beschikbaarOp.HasValue)
            {
                // We zoeken tafels die GEEN reservering hebben op dit tijdstip (+/- 2 uur buffer)
                // En we negeren geannuleerde reserveringen (!r.Cancelled)
                query = query.Where(t => !t.Reserveringen.Any(r =>
                    !r.Cancelled &&
                    r.DatumTijd < beschikbaarOp.Value.AddHours(2) && // Reservering begint voor einde tijdslot
                    r.DatumTijd > beschikbaarOp.Value.AddHours(-2)   // Reservering eindigt na start tijdslot
                ));
            }

            return await query.ToListAsync();
        }

        // GET: api/Tafels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tafel>> GetTafel(int id)
        {
            var tafel = await _context.Tafels.FindAsync(id);
            if (tafel == null) return NotFound();
            return tafel;
        }

        // POST: api/Tafels
        [HttpPost]
        public async Task<ActionResult<Tafel>> PostTafel(TafelInputDto input)
        {
            // Zet de simpele DTO om naar het echte database model
            var tafel = new Tafel
            {
                Tafelnummer = input.Tafelnummer,
                AantalPlaatsen = input.AantalPlaatsen
                // Reserveringen laten we leeg
            };

            _context.Tafels.Add(tafel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTafel), new { id = tafel.TafelID }, tafel);
        }

        // PUT: api/Tafels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTafel(int id, TafelInputDto input)
        {
            var bestaandeTafel = await _context.Tafels.FindAsync(id);

            if (bestaandeTafel == null)
            {
                return NotFound();
            }

            // Alleen de eigenschappen updaten die mogen veranderen
            bestaandeTafel.Tafelnummer = input.Tafelnummer;
            bestaandeTafel.AantalPlaatsen = input.AantalPlaatsen;

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

        // DELETE: api/Tafels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTafel(int id)
        {
            var tafel = await _context.Tafels
                .Include(t => t.Reserveringen)
                .FirstOrDefaultAsync(t => t.TafelID == id);

            if (tafel == null) return NotFound();

            // Prevent deleting a table if it has history
            if (tafel.Reserveringen.Any())
            {
                return BadRequest("Kan tafel niet verwijderen: Er zijn reserveringen aan gekoppeld.");
            }

            _context.Tafels.Remove(tafel);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}