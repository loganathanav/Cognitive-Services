using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Analytics.Models;

namespace Services.Analytics.Controllers
{
    [Produces("application/json")]
    [Route("api/Medias")]
    public class MediasController : Controller
    {
        private readonly ZetronContext _context;

        public MediasController(ZetronContext context)
        {
            _context = context;
        }

        // GET: api/Medias
        [HttpGet]
        public IEnumerable<ZetronTrnMediaDetails> GetZetronTrnMediaDetails()
        {
            var zetronMedias = _context.ZetronTrnMediaDetails.ToList();
            for (int mediaIndex = 0; mediaIndex < zetronMedias.Count(); mediaIndex++)
            {
                var tags = _context.ZetronTrnFrameTags.Where(tag => tag.MediaId == zetronMedias[mediaIndex].MediaId).ToList();
                if (tags != null && tags.Count > 0)
                {
                    zetronMedias[mediaIndex].ZetronTrnFrameTags = (ICollection<ZetronTrnFrameTags>)tags;
                }

            }

            return zetronMedias;
        }

        // GET: api/Medias/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetZetronTrnMediaDetails([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronTrnMediaDetails = await _context.ZetronTrnMediaDetails.SingleOrDefaultAsync(m => m.MediaId == id);

            if (zetronTrnMediaDetails == null)
            {
                return NotFound();
            }

            var tags = _context.ZetronTrnFrameTags.Where(tag => tag.MediaId == zetronTrnMediaDetails.MediaId).ToList();
            if (tags != null && tags.Count > 0)
            {
                zetronTrnMediaDetails.ZetronTrnFrameTags = (ICollection<ZetronTrnFrameTags>)tags;
            }

            return Ok(zetronTrnMediaDetails);
        }

        // PUT: api/Medias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutZetronTrnMediaDetails([FromRoute] int id, [FromBody] ZetronTrnMediaDetails zetronTrnMediaDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != zetronTrnMediaDetails.MediaId)
            {
                return BadRequest();
            }

            _context.Entry(zetronTrnMediaDetails).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ZetronTrnMediaDetailsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }

        // POST: api/Medias
        [HttpPost]
        public async Task<IActionResult> PostZetronTrnMediaDetails([FromBody] ZetronTrnMediaDetails zetronTrnMediaDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ZetronTrnMediaDetails.Add(zetronTrnMediaDetails);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetZetronTrnMediaDetails", new { id = zetronTrnMediaDetails.MediaId }, zetronTrnMediaDetails);
        }

        // DELETE: api/Medias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZetronTrnMediaDetails([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronMedias = await _context.ZetronTrnMediaDetails.SingleOrDefaultAsync(m => m.MediaId == id);
            if (zetronMedias == null)
            {
                return NotFound();
            }

            var zetronTags = _context.ZetronTrnFrameTags.Where(m => m.MediaId == id);
            if (zetronTags != null)
            {
                _context.ZetronTrnFrameTags.RemoveRange(zetronTags);
            }

            _context.ZetronTrnMediaDetails.Remove(zetronMedias);
            await _context.SaveChangesAsync();

            return Ok("success");
        }

        private bool ZetronTrnMediaDetailsExists(int id)
        {
            return _context.ZetronTrnMediaDetails.Any(e => e.MediaId == id);
        }
    }
}