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
        // Combineert alles: ID, Plaatsen, Datum
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tafel>>> GetTafels(
            [FromQuery] int? id,
            [FromQuery] int? aantalPlaatsen,
            [FromQuery] DateTime? beschikbaarOp)
        {
            var query = _context.Tafels.AsQueryable();

            // 1. Filter op ID
            if (id.HasValue)
            {
                query = query.Where(t => t.TafelID == id.Value);
            }

            // 2. Filter op Aantal Plaatsen
            if (aantalPlaatsen.HasValue)
            {
                query = query.Where(t => t.AantalPlaatsen >= aantalPlaatsen.Value);
            }

            // 3. Filter op Beschikbaarheid
            if (beschikbaarOp.HasValue)
            {
                query = query.Where(t => !t.Reserveringen.Any(r =>
                    !r.Cancelled &&
                    r.DatumTijd < beschikbaarOp.Value.AddHours(2) &&
                    r.DatumTijd > beschikbaarOp.Value.AddHours(-2)
                ));
            }

            var result = await query.ToListAsync();

            if (id.HasValue && !result.Any())
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST: api/Tafels
        [HttpPost]
        public async Task<ActionResult<Tafel>> PostTafel(TafelInputDto input)
        {
            var tafel = new Tafel
            {
                Tafelnummer = input.Tafelnummer,
                AantalPlaatsen = input.AantalPlaatsen
            };

            _context.Tafels.Add(tafel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTafels), new { id = tafel.TafelID }, tafel);
        }

        // PUT: api/Tafels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTafel(int id, TafelInputDto input)
        {
            var bestaandeTafel = await _context.Tafels.FindAsync(id);
            if (bestaandeTafel == null) return NotFound();

            bestaandeTafel.Tafelnummer = input.Tafelnummer;
            bestaandeTafel.AantalPlaatsen = input.AantalPlaatsen;

            await _context.SaveChangesAsync();
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