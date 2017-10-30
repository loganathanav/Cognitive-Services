using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Analytics.Interfaces;
using Services.Analytics.Models;

namespace Services.Analytics.Controllers
{
    [Produces("application/json")]
    [Route("api/Location")]
    public class LocationController : Controller
    {
        private readonly ZetronContext _context;
        private readonly IDrone _drone;
        public LocationController(ZetronContext context, IDrone drone)
        {
            _context = context;
            _drone = drone;
        }

        // GET: api/Locations
        [HttpGet]
        public DroneDetail GetZetronTrnDroneLocations()
        {
            return _drone.GetCurrentLocationDetail();
        }

        // GET: api/Locations/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetZetronTrnDroneLocations([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronTrnDroneLocations = await _context.ZetronTrnDroneLocations.SingleOrDefaultAsync(m => m.MediaID == id);

            if (zetronTrnDroneLocations == null)
            {
                return NotFound();
            }

            return Ok(zetronTrnDroneLocations);
        }

        // PUT: api/Locations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutZetronTrnDroneLocations([FromRoute] int id, [FromBody] ZetronTrnDroneLocations zetronTrnDroneLocations)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != zetronTrnDroneLocations.LocationID)
            {
                return BadRequest();
            }

            _context.Entry(zetronTrnDroneLocations).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ZetronTrnDroneLocationsExists(id))
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

        // POST: api/Locations
        [HttpPost]
        public async Task<IActionResult> PostZetronTrnDroneLocations([FromBody] ZetronTrnDroneLocations zetronTrnDroneLocations)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ZetronTrnDroneLocations.Add(zetronTrnDroneLocations);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetZetronTrnDroneLocations", new { id = zetronTrnDroneLocations.LocationID }, zetronTrnDroneLocations);
        }

        // DELETE: api/Locations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZetronTrnDroneLocations([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronTrnDroneLocations = await _context.ZetronTrnDroneLocations.SingleOrDefaultAsync(m => m.LocationID == id);
            if (zetronTrnDroneLocations == null)
            {
                return NotFound();
            }

            _context.ZetronTrnDroneLocations.Remove(zetronTrnDroneLocations);
            await _context.SaveChangesAsync();

            return Ok(zetronTrnDroneLocations);
        }

        private bool ZetronTrnDroneLocationsExists(int id)
        {
            return _context.ZetronTrnDroneLocations.Any(e => e.LocationID == id);
        }
    }
}