using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using estimateProjectx.Models;
using estimateProjectx.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace estimateProjectx.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var sessionsWithVotes = await _context.Sessions
                .Include(s => s.User)
                .Include(s => s.Votes)
                .ToListAsync();

            // Calculate and set VotesCount for each session
            foreach (var session in sessionsWithVotes)
            {
                session.VotesCount = session.Votes?.Count(v => v.IdentityUserId != null) ?? 0;

                // Check if all users have voted
                var uniqueUserIds = session.Votes?.Select(v => v.IdentityUserId).Distinct().ToList();
                var allUsersVoted = uniqueUserIds.Count == _userManager.Users.Count();
            }

            return View(sessionsWithVotes);
        }



        // ... (other actions like Privacy, Error, etc.)

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,IdentityUserId")] Session session)
        {
            if (ModelState.IsValid)
            {
                session.CreatedAt = DateTime.UtcNow;
                session.Votes = new List<Vote>();
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Log ModelState errors for debugging
            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
            }

            return View(session);
        }


        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Sessions == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", session.IdentityUserId);
            return View(session);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,IdentityUserId")] Session session)
        {
            if (id != session.Id)
            {
                return NotFound();
            }

            // Retrieve the current logged-in user
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the session belongs to the current logged-in user
            if (session.IdentityUserId != currentUser.Id)
            {
                return Forbid(); // Or return another appropriate result like a Forbidden page
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.Id))
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

            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", session.IdentityUserId);
            return View(session);
        }


        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }



        [Authorize]
        public async Task<IActionResult> EndSession(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the session belongs to the current logged-in user
            if (session.IdentityUserId != currentUser.Id)
            {
                return Forbid(); // Or return another appropriate result like a Forbidden page
            }

            return View(session);
        }


        [Authorize]
        [HttpPost, ActionName("EndSession")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Sessions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Sessions'  is null.");
            }
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return (_context.Sessions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
