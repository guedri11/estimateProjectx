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

        // GET: api/Votes/GetVotes/{sessionId}
        [HttpGet("api/Votes/GetVotes/{sessionId}")]
        public IActionResult GetVotes(int sessionId)
        {
            var votes = _context.Vote.Where(v => v.SessionId == sessionId).ToList();

            if (votes == null || votes.Count == 0)
            {
                return NotFound(new { Message = "No votes found for this session." });
            }

            return Ok(votes);
        }

        [HttpPost]
        [Route("api/Votes/SubmitVote")]
        public async Task<IActionResult> SubmitVote([FromBody] Vote vote)
        {
            // Check if the vote value is within the acceptable range
            if (vote.VoteValue < 1 || vote.VoteValue > 13 ||
                (vote.VoteValue != 1 && vote.VoteValue != 3 && vote.VoteValue != 5 && vote.VoteValue != 8 && vote.VoteValue != 13))
            {
                return BadRequest("Invalid vote value. Please vote using 1, 3, 5, 8, or 13.");
            }

            // Get total number of registered users
            int totalUsers = await _context.Users.CountAsync();

            // Calculate maximum votes allowed (total registered users + 10 guest votes)
            int maxVotesAllowed = totalUsers + 10;

            // Get the current total number of votes for the session
            int currentVotes = await _context.Vote.CountAsync(v => v.SessionId == vote.SessionId);

            // Check if the current votes have reached the limit
            if (currentVotes >= maxVotesAllowed)
            {
                return BadRequest("Free vote capacity reached. Please log in to continue voting.");
            }

            // Handle guest votes (ensure they don't exceed 10 guest votes)
            if (string.IsNullOrEmpty(vote.IdentityUserId))
            {
                // Check the number of guest votes for the session
                int guestVotes = await _context.Vote.CountAsync(v => v.SessionId == vote.SessionId && string.IsNullOrEmpty(v.IdentityUserId));

                if (guestVotes >= 10)
                {
                    return BadRequest("Guest vote capacity reached. Please log in to continue voting.");
                }

                // Mark this vote as a guest vote by setting IdentityUserId to null
                vote.IdentityUserId = null;
            }

            // Add the vote to the database
            _context.Vote.Add(vote);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            return Ok(new { Message = "Vote submitted successfully." });
        }





        // GET: api/Votes/GetAverageVote/{sessionId}
        [HttpGet("api/Votes/GetAverageVote/{sessionId}")]
        public IActionResult GetAverageVote(int sessionId)
        {
            var votes = _context.Vote.Where(v => v.SessionId == sessionId).Select(v => v.VoteValue).ToList();

            if (votes == null || votes.Count == 0)
            {
                return NotFound(new { Message = "No votes found for this session." });
            }

            var averageVote = votes.Average();
            return Ok(new { AverageVote = averageVote });
        }


    }
}
