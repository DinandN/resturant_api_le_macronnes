using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.Models;
using LeMacronnesResturauntAPI.DTOs;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gerecht>>> GetMenu()
        {
            return await _context.Gerechten.ToListAsync();
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
        [HttpPost]
        [HttpPost]
        public async Task<ActionResult<Gerecht>> PostGerecht(Gerecht gerecht)
        {
            gerecht.GerechtID = 0;

            _context.Gerechten.Add(gerecht);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGerecht), new { id = gerecht.GerechtID }, gerecht);
        }

        // PUT: api/Menu/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGerecht(int id, Gerecht gerecht)
        {
            // Controleer of de ID in de URL overeenkomt met de ID in de body
            if (id != gerecht.GerechtID)
            {
                return BadRequest("ID in URL komt niet overeen met ID in body.");
            }

            // Markeer de entity als gewijzigd
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

        // Hulpmethode om te checken of een gerecht bestaat
        private bool GerechtExists(int id)
        {
            return _context.Gerechten.Any(e => e.GerechtID == id);
        }
    }
}