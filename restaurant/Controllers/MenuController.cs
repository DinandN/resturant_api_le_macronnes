using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.DTOs;
using LeMacronnesResturauntAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMacronnesResturauntAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public MenuController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/Menu
        // Optioneel zoeken: api/Menu?naam=bief&allergenen=gluten
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gerecht>>> GetMenu(
            [FromQuery] int? id,
            [FromQuery] string? naam,
            [FromQuery] string? allergenen)
        {
            var query = _context.Gerechten.AsQueryable();

            // Filter op ID als die is ingevuld
            if (id.HasValue)
            {
                query = query.Where(g => g.GerechtID == id.Value);
            }

            // Filter op Naam als die is ingevuld
            if (!string.IsNullOrEmpty(naam))
            {
                query = query.Where(g => g.Naam.Contains(naam));
            }

            // Filter op Allergenen als die is ingevuld
            if (!string.IsNullOrEmpty(allergenen))
            {
                query = query.Where(g => g.Allergenen.Contains(allergenen));
            }

            return await query.ToListAsync();
        }

        // GET: api/Menu/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Gerecht>> GetGerecht(int id)
        {
            var gerecht = await _context.Gerechten.FindAsync(id);

            if (gerecht == null)
            {
                return NotFound();
            }

            return gerecht;
        }

        // POST: api/Menu
        // Gebruikt DTO zodat ID niet handmatig ingevuld kan worden
        [HttpPost]
        public async Task<ActionResult<Gerecht>> PostGerecht(GerechtInputDto input)
        {
            var gerecht = new Gerecht
            {
                Naam = input.Naam,
                Omschrijving = input.Omschrijving,
                Prijs = input.Prijs,
                Allergenen = input.Allergenen
            };

            _context.Gerechten.Add(gerecht);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGerecht), new { id = gerecht.GerechtID }, gerecht);
        }

        // PUT: api/Menu/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGerecht(int id, Gerecht gerecht)
        {
            if (id != gerecht.GerechtID)
            {
                return BadRequest("ID in URL komt niet overeen met ID in de body.");
            }

            _context.Entry(gerecht).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GerechtExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Menu/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGerecht(int id)
        {
            var gerecht = await _context.Gerechten.FindAsync(id);
            if (gerecht == null)
            {
                return NotFound();
            }

            _context.Gerechten.Remove(gerecht);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GerechtExists(int id)
        {
            return _context.Gerechten.Any(e => e.GerechtID == id);
        }
    }
}