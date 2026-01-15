
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tafel>>> GetTafels()
        {
            return await _context.Tafels.ToListAsync();
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
                // Reserveringen laten we leeg, want een nieuwe tafel heeft die nog niet
            };

            _context.Tafels.Add(tafel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTafel), new { id = tafel.TafelID }, tafel);
        }

        // PUT: api/Tafels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTafel(int id, TafelInputDto input)
        {
            // Eerst de echte tafel ophalen uit de database
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
        // We check if the table has reservations before deleting
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTafel(int id)
        {
            var tafel = await _context.Tafels.Include(t => t.Reserveringen).FirstOrDefaultAsync(t => t.TafelID == id);
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