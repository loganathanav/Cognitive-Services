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
    [Route("api/Incidents")]
    public class IncidentsController : Controller
    {
        private readonly ZetronContext _context;
        private readonly MediaContext _mediaContext;

        public IncidentsController(ZetronContext context, MediaContext mediaContext)
        {
            _context = context;
            _mediaContext = mediaContext;
        }

        // GET: api/Incidents
        [HttpGet]
        public async Task<IEnumerable<ZetronMstIncidents>> GetZetronMstIncidents()
        {
            var zetronMstIncidents = _context.ZetronMstIncidents.ToList();
            for (int i = 0; i < zetronMstIncidents.Count(); i++)
            {
                if (zetronMstIncidents[i].ZetronTrnMediaDetails.Count == 0)
                {
                    var zetronMedias = await _context.ZetronTrnMediaDetails.SingleOrDefaultAsync(m => m.IncidentId == zetronMstIncidents[i].IncidentId);
                    if (zetronMedias != null)
                    {
                        zetronMstIncidents[i].ZetronTrnMediaDetails.Add(zetronMedias);
                    }
                }

                if (zetronMstIncidents[i].ZetronTrnMediaDetails.Count > 0)
                {
                    var medias = zetronMstIncidents[i].ZetronTrnMediaDetails.ToList();
                    for (int mediaIndex = 0; mediaIndex < medias.Count; mediaIndex++)
                    {
                        var tags = _context.ZetronTrnFrameTags.Where(tag => tag.MediaId == medias[mediaIndex].MediaId).ToList();
                        if (tags != null && tags.Count > 0)
                        {
                            medias[mediaIndex].ZetronTrnFrameTags = (ICollection<ZetronTrnFrameTags>)tags;
                        }
                    }
                }
            }
            return zetronMstIncidents;
        }

        // GET: api/Incidents/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetZetronMstIncidents([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronMstIncidents = await _context.ZetronMstIncidents.SingleOrDefaultAsync(m => m.IncidentId == id);

            if (zetronMstIncidents == null)
            {
                return NotFound();
            }

            if (zetronMstIncidents.ZetronTrnMediaDetails.Count == 0)
            {
                var zetronMedias = await _context.ZetronTrnMediaDetails.SingleOrDefaultAsync(m => m.IncidentId == id);
                if (zetronMedias != null)
                {
                    zetronMstIncidents.ZetronTrnMediaDetails.Add(zetronMedias);
                }
            }
            if (zetronMstIncidents.ZetronTrnMediaDetails.Count > 0)
            {
                var medias = zetronMstIncidents.ZetronTrnMediaDetails.ToList();
                for (int i = 0; i < medias.Count; i++)
                {
                    var tags = _context.ZetronTrnFrameTags.Where(t => t.MediaId == medias[i].MediaId).ToList();
                    if (tags != null && tags.Count > 0)
                    {
                        medias[i].ZetronTrnFrameTags = (ICollection<ZetronTrnFrameTags>)tags;
                    }
                }
            }

            return Ok(zetronMstIncidents);
        }

        // PUT: api/Incidents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutZetronMstIncidents([FromRoute] int id, [FromBody] ZetronMstIncidents zetronMstIncidents)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != zetronMstIncidents.IncidentId)
            {
                return BadRequest();
            }

            try
            {
                var recordtoUpdate = _context.ZetronMstIncidents.Single(i => i.IncidentId == id);
                recordtoUpdate.Status = zetronMstIncidents.Status;
                await _context.SaveChangesAsync();

                switch (zetronMstIncidents.Status)
                {
                    case (int)IncidentStatus.Processing:
                        //trigger job
                        _mediaContext.TriggerJob(zetronMstIncidents.Title);
                        break;
                    case (int)IncidentStatus.Stopped:
                        _mediaContext.StopAzureProcess();
                        break;
                    case (int)IncidentStatus.Deactivated:
                        _mediaContext.StopAzureProcess();
                        break;
                    default:
                        break;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ZetronMstIncidentsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetZetronMstIncidents", new { id = zetronMstIncidents.IncidentId }, zetronMstIncidents);
        }

        // POST: api/Incidents
        [HttpPost]
        public async Task<IActionResult> PostZetronMstIncidents([FromBody] ZetronMstIncidents zetronMstIncidents)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            _context.ZetronMstIncidents.Add(zetronMstIncidents);
            await _context.SaveChangesAsync();

            _mediaContext.StartAzureProcess(zetronMstIncidents);

            return CreatedAtAction("GetZetronMstIncidents", new { id = zetronMstIncidents.IncidentId }, zetronMstIncidents);
        }

        // DELETE: api/Incidents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZetronMstIncidents([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var zetronMstIncidents = await _context.ZetronMstIncidents.SingleOrDefaultAsync(m => m.IncidentId == id);
            if (zetronMstIncidents == null)
            {
                return NotFound();
            }

            var zetronMedias = _context.ZetronTrnMediaDetails.Where(m => m.IncidentId == id).ToList();

            if (zetronMedias != null && zetronMedias.Count > 0)
            {
                for (int mediaIndex = 0; mediaIndex < zetronMedias.Count; mediaIndex++)
                {
                    var zetronTags = _context.ZetronTrnFrameTags.Where(m => m.MediaId == zetronMedias[mediaIndex].MediaId);
                    if (zetronTags != null)
                    {
                        _context.ZetronTrnFrameTags.RemoveRange(zetronTags);
                    }
                }
                _context.ZetronTrnMediaDetails.RemoveRange(zetronMedias);
            }

            _context.ZetronMstIncidents.Remove(zetronMstIncidents);
            await _context.SaveChangesAsync();

            return Ok("success");
        }

        private bool ZetronMstIncidentsExists(int id)
        {
            return _context.ZetronMstIncidents.Any(e => e.IncidentId == id);
        }
    }
}