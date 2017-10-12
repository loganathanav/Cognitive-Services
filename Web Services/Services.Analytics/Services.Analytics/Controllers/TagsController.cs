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
    [Route("api/Tags")]
    public class TagsController : Controller
    {
        private readonly ZetronContext _context;

        public TagsController(ZetronContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public IEnumerable<ZetronTrnFrameTags> GetZetronTrnFrameTags()
        {
            return _context.ZetronTrnFrameTags;
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetZetronTrnFrameTags([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronTrnFrameTags = await _context.ZetronTrnFrameTags.SingleOrDefaultAsync(m => m.FrameId == id);

            if (zetronTrnFrameTags == null)
            {
                return NotFound();
            }

            return Ok(zetronTrnFrameTags);
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutZetronTrnFrameTags([FromRoute] int id, [FromBody] ZetronTrnFrameTags zetronTrnFrameTags)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != zetronTrnFrameTags.FrameId)
            {
                return BadRequest();
            }

            _context.Entry(zetronTrnFrameTags).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ZetronTrnFrameTagsExists(id))
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

        // POST: api/Tags
        [HttpPost]
        public async Task<IActionResult> PostZetronTrnFrameTags([FromBody] ZetronTrnFrameTags zetronTrnFrameTags)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ZetronTrnFrameTags.Add(zetronTrnFrameTags);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetZetronTrnFrameTags", new { id = zetronTrnFrameTags.FrameId }, zetronTrnFrameTags);
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZetronTrnFrameTags([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronTrnFrameTags = await _context.ZetronTrnFrameTags.SingleOrDefaultAsync(m => m.FrameId == id);
            if (zetronTrnFrameTags == null)
            {
                return NotFound();
            }

            _context.ZetronTrnFrameTags.Remove(zetronTrnFrameTags);
            await _context.SaveChangesAsync();

            return Ok("success");
        }

        private bool ZetronTrnFrameTagsExists(int id)
        {
            return _context.ZetronTrnFrameTags.Any(e => e.FrameId == id);
        }
    }
}