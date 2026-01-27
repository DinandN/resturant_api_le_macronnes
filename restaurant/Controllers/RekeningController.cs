using LeMacronnesResturauntAPI.Data;
using LeMacronnesResturauntAPI.DTOs;
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
        public async Task<ActionResult<IEnumerable<RekeningOutputDto>>> GetRekening(
            [FromQuery] int? rekeningId,
            [FromQuery] int? tafelId,
            [FromQuery] int? boekingId)
        {
            var query = _context.Rekeningen
                .Include(r => r.Reservering)
                .Include(r => r.Bestellingen)
                    .ThenInclude(b => b.BestelRegels)
                        .ThenInclude(br => br.Gerecht)
                .AsQueryable();

            if (rekeningId.HasValue)
            {
                query = query.Where(r => r.RekeningID == rekeningId);
            }
            else if (tafelId.HasValue)
            {
                // We ensure Reservering is not null to prevent matching orphaned bills
                query = query.Where(r => r.Reservering != null && r.Reservering.TafelID == tafelId && r.Status == "Open");
            }
            else if (boekingId.HasValue)
            {
                query = query.Where(r => r.Reservering != null && r.Reservering.BoekingID == boekingId);
            }
            else
            {
                return BadRequest("Geef een rekeningId, tafelId of boekingId mee.");
            }

            // The Select fails if we try to read a NULL into an int. 
            var rekeningen = await query.Select(r => new RekeningOutputDto
            {
                RekeningID = r.RekeningID,
                Status = r.Status ?? "Onbekend", 
                TotaalBetaald = r.TotaalBetaald,

                // This handles cases where the 'Reservering' record is missing (Broken FK).
                TafelID = (int?)r.Reservering.TafelID ?? 0,
                BoekingID = (int?)r.Reservering.BoekingID ?? 0,

                Items = r.Bestellingen
                    .SelectMany(b => b.BestelRegels)
                    .Select(br => new RekeningItemDto
                    {
                        // Handle if Gerecht has been deleted
                        GerechtNaam = br.Gerecht != null ? br.Gerecht.Naam : "Verwijderd Gerecht",
                        Aantal = (int?)br.Aantal ?? 0,

                        // Handle if Gerecht or Price is missing
                        PrijsPerStuk = br.Gerecht != null ? br.Gerecht.Prijs : 0
                    }).ToList()
            }).ToListAsync();

            if (!rekeningen.Any())
            {
                return NotFound("Geen rekening gevonden.");
            }

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

            decimal totaalTeBetalen = rekening.Bestellingen
                .SelectMany(b => b.BestelRegels)
                .Sum(br => br.Aantal * (br.Gerecht != null ? br.Gerecht.Prijs : 0));

            if (input.TotaalBetaald < 0)
            {
                return BadRequest("Het betaalde bedrag mag niet negatief zijn.");
            }

            if (input.TotaalBetaald < totaalTeBetalen)
            {
                return BadRequest(new
                {
                    Message = "Betaling mislukt: Het bedrag is lager dan de rekening.",
                    HuidigBetaald = input.TotaalBetaald,
                    TotaalVereist = totaalTeBetalen,
                    NogTeBetalen = totaalTeBetalen - input.TotaalBetaald
                });
            }

            rekening.TotaalBetaald = input.TotaalBetaald;
            rekening.BetaalMethode = input.BetaalMethode;
            rekening.Status = "Betaald";

            await _context.SaveChangesAsync();

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