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
        // Voorbeeld: api/Menu?id=1
        // Voorbeeld: api/Menu?allergenen=Gluten,Pinda (Sluit gerechten met deze allergenen UIT)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Gerecht>>> GetMenu(
            [FromQuery] int? id,
            [FromQuery] string? naam,
            [FromQuery] string? allergenen)
        {
            var query = _context.Gerechten.AsQueryable();

            // 1. Filter op ID
            if (id.HasValue)
            {
                query = query.Where(g => g.GerechtID == id.Value);
            }

            // 2. Filter op Naam
            if (!string.IsNullOrEmpty(naam))
            {
                query = query.Where(g => g.Naam.Contains(naam));
            }

            // 3. Filter op Allergenen (Does Not Contain / Exclude List)
            // Verwacht input als: "Gluten,Lactose"
            if (!string.IsNullOrEmpty(allergenen))
            {
                // Split de string in een lijst (bijv: ["Gluten", "Lactose"])
                var teVermijdenAllergenen = allergenen.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (var teVermijden in teVermijdenAllergenen)
                {
                    // Voor elk item in de lijst, voeg een filter toe dat zegt: 
                    // Het gerecht mag dit allergeen NIET bevatten.
                    query = query.Where(g => !g.Allergenen.Contains(teVermijden));
                }
            }

            var result = await query.ToListAsync();

            // Als er op ID werd gezocht en niets gevonden, geef NotFound
            if (id.HasValue && !result.Any())
            {
                return NotFound();
            }

            return Ok(result);
        }

        // POST: api/Menu
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

            // Omdat we geen aparte GetById meer hebben, verwijzen we naar de lijst filter
            return CreatedAtAction(nameof(GetMenu), new { id = gerecht.GerechtID }, gerecht);
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
                if (!GerechtExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Menu/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGerecht(int id)
        {
            var gerecht = await _context.Gerechten.FindAsync(id);
            if (gerecht == null) return NotFound();

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