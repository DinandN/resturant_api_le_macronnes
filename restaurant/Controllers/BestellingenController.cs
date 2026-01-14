using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.Models;
using LeMacronnesResturauntAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeMacronnesResturauntAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BestellingenController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public BestellingenController(RestaurantContext context)
        {
            _context = context;
        }

        // POST: api/Bestellingen
        [HttpPost]
        public async Task<ActionResult> PlaatsBestelling(BestellingInputDto input)
        {
            var rekening = await _context.Rekeningen.FindAsync(input.RekeningID);
            if (rekening == null || rekening.Status == "Betaald")
            {
                return BadRequest("Rekening niet gevonden of al betaald.");
            }

            var bestelling = new Bestelling
            {
                RekeningID = input.RekeningID,
                BestelRegels = new List<BestelRegel>()
            };

            foreach (var item in input.Items)
            {
                var bestelRegel = new BestelRegel
                {
                    GerechtID = item.GerechtID,
                    Aantal = item.Aantal,
                    Aanpassing = item.Aanpassing
                };
                bestelling.BestelRegels.Add(bestelRegel);
            }

            _context.Bestellingen.Add(bestelling);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Bestelling succesvol geplaatst", BestellingID = bestelling.BestellingID });
        }

        // GET: api/Bestellingen/Rekening/5
        [HttpGet("Rekening/{rekeningId}")]
        public async Task<ActionResult> GetRekeningDetails(int rekeningId)
        {
            var details = await _context.Rekeningen
                .Where(r => r.RekeningID == rekeningId)
                .Include(r => r.Bestellingen)
                    .ThenInclude(b => b.BestelRegels)
                        .ThenInclude(br => br.Gerecht)
                .Select(r => new
                {
                    r.RekeningID,
                    r.Status,
                    TotaalBesteld = r.Bestellingen
                        .SelectMany(b => b.BestelRegels)
                        .Sum(br => br.Aantal * br.Gerecht.Prijs) // LINQ berekening
                })
                .FirstOrDefaultAsync();

            if (details == null) return NotFound();

            return Ok(details);
        }
    }
}