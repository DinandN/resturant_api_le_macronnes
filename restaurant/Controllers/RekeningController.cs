using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMacronnesResturauntAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RekeningenController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public RekeningenController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/Rekeningen
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rekening>>> GetRekeningen()
        {
            return await _context.Rekeningen.ToListAsync();
        }

        // GET: api/Rekeningen/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Rekening>> GetRekening(int id)
        {
            var rekening = await _context.Rekeningen
                .Include(r => r.Bestellingen)
                .FirstOrDefaultAsync(r => r.RekeningID == id);

            if (rekening == null) return NotFound();
            return rekening;
        }

        // PUT: api/Rekeningen/5/Afsluiten
        // Used to pay the bill
        [HttpPut("{id}/Afsluiten")]
        public async Task<IActionResult> BetaalRekening(int id, string betaalMethode)
        {
            var rekening = await _context.Rekeningen
                 .Include(r => r.Bestellingen)
                    .ThenInclude(b => b.BestelRegels)
                        .ThenInclude(br => br.Gerecht)
                 .FirstOrDefaultAsync(r => r.RekeningID == id);

            if (rekening == null) return NotFound();

            // Calculate Total automatically based on orders
            decimal totaal = rekening.Bestellingen
                .SelectMany(b => b.BestelRegels)
                .Sum(br => br.Aantal * br.Gerecht.Prijs);

            rekening.Status = "Betaald";
            rekening.BetaalMethode = betaalMethode;
            rekening.TotaalBetaald = totaal;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Rekening betaald en afgesloten", Totaal = totaal });
        }

        // DELETE: api/Rekeningen/5
        [HttpDelete("{id}")]
        public ActionResult DeleteRekening(int id)
        {
            // Explicitly forbid deleting financial data
            return StatusCode(405, "Het verwijderen van rekeningen is niet toegestaan vanwege fiscale redenen.");
        }
    }
}