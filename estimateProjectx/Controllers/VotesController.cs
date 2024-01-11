using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using estimateProjectx.Data;
using estimateProjectx.Models;
using Microsoft.AspNetCore.Authorization;

namespace estimateProjectx.Controllers
{
    public class VotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Votes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Vote.Include(v => v.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Votes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vote == null)
            {
                return NotFound();
            }

            var vote = await _context.Vote
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vote == null)
            {
                return NotFound();
            }

            return View(vote);
        }

        // GET: Votes/Create
        public IActionResult Create(int sessionId) // Accepts the sessionId parameter
        {
            ViewData["SessionId"] = sessionId;
            return View();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Votes/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VoteValue,SessionId,IdentityUserId")] Vote vote)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    if (ModelState.IsValid) 
                    {
                    vote.IdentityUserId = userId;

                    _context.Add(vote);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", "Home", new { id = vote.SessionId }); // Redirect to session details after vote
                    }
                }
            }
            return View(vote);
        }



        // GET: Votes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vote == null)
            {
                return NotFound();
            }

            var vote = await _context.Vote.FindAsync(id);
            if (vote == null)
            {
                return NotFound();
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", vote.IdentityUserId);
            return View(vote);
        }

        // POST: Votes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SessionId,VoteValue,IdentityUserId")] Vote vote)
        {
            if (id != vote.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vote);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoteExists(vote.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", vote.IdentityUserId);
            return View(vote);
        }

        // GET: Votes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vote == null)
            {
                return NotFound();
            }

            var vote = await _context.Vote
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vote == null)
            {
                return NotFound();
            }

            return View(vote);
        }

        // POST: Votes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vote == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Vote'  is null.");
            }
            var vote = await _context.Vote.FindAsync(id);
            if (vote != null)
            {
                _context.Vote.Remove(vote);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("Get", "Post")]
        public IActionResult VerifyUniqueVote(int voteValue, int sessionId)
        {
            var dbContext = (ApplicationDbContext)HttpContext.RequestServices.GetService(typeof(ApplicationDbContext));

            var existingVote = dbContext.Vote
                .FirstOrDefault(v => v.SessionId == sessionId && v.VoteValue == voteValue);

            return Json(existingVote == null);
        }

        private bool VoteExists(int id)
        {
            return _context.Vote.Any(e => e.Id == id);
        }
    }
}
