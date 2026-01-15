using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.DTOs; // Vergeet deze niet
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
        public async Task<ActionResult<IEnumerable<object>>> GetRekeningen()
        {
            var rekeningen = await _context.Rekeningen
                .Include(r => r.Bestellingen)
                    .ThenInclude(b => b.BestelRegels)
                        .ThenInclude(br => br.Gerecht)
                .Select(r => new
                {
                    r.RekeningID,
                    r.Status,
                    r.BetaalMethode,
                    ReedsBetaald = r.TotaalBetaald,
                    // Live berekening van wat het totaal zou moeten zijn
                    TotaalBedrag = r.Bestellingen.SelectMany(b => b.BestelRegels).Sum(br => br.Aantal * br.Gerecht.Prijs)
                })
                .ToListAsync();

            return Ok(rekeningen);
        }

        [HttpPut("{id}/Betalen")]
        public async Task<IActionResult> UpdateBetaling(int id, RekeningBetalingDto input)
        {
            var rekening = await _context.Rekeningen
                    .Include(r => r.Bestellingen)
                    .ThenInclude(b => b.BestelRegels)
                        .ThenInclude(br => br.Gerecht)
                    .FirstOrDefaultAsync(r => r.RekeningID == id);

            if (rekening == null) return NotFound();

            // 1. Calculate the total cost first
            decimal totaalTeBetalen = rekening.Bestellingen
                .SelectMany(b => b.BestelRegels)
                .Sum(br => br.Aantal * br.Gerecht.Prijs);

            // 2. CHECK: Is the input negative? (Sanity check)
            if (input.TotaalBetaald < 0)
            {
                return BadRequest("Het betaalde bedrag mag niet negatief zijn.");
            }

            // 3. CHECK: Is the payment less than the total cost?
            // "Make sure the amount doesn't go into the minus" logic
            if (input.TotaalBetaald < totaalTeBetalen)
            {
                return BadRequest(new
                {
                    Message = "Betaling mislukt: Het bedrag is lager dan de rekening.",
                    HuidigBetaald = input.TotaalBetaald,
                    TotaalVereist = totaalTeBetalen, // <--- Here is the total they need to pay
                    NogTeBetalen = totaalTeBetalen - input.TotaalBetaald
                });
            }

            // 4. Update the data (Only happens if checks pass)
            rekening.TotaalBetaald = input.TotaalBetaald;
            rekening.BetaalMethode = input.BetaalMethode;

            // Since we checked above, we know it is fully paid
            rekening.Status = "Betaald";

            await _context.SaveChangesAsync();

            // 5. Return success (optionally return change/tip info)
            return Ok(new
            {
                Message = "Betaling succesvol verwerkt",
                Status = rekening.Status,
                TotaalBedrag = totaalTeBetalen,
                WisselgeldOfFooi = input.TotaalBetaald - totaalTeBetalen
            });
        }
    }
}