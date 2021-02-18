using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wyrobot.Web.Data;
using Wyrobot.Web.Models;

namespace Wyrobot.Web.Controllers
{
    public class LevelRewardsController : Controller
    {
        private readonly WyrobotWebContext _context;

        public LevelRewardsController(WyrobotWebContext context)
        {
            _context = context;
        }

        // GET: LevelRewards
        public async Task<IActionResult> Index()
        {
            return View(await _context.LevelReward.ToListAsync());
        }

        // GET: LevelRewards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelReward = await _context.LevelReward
                .FirstOrDefaultAsync(m => m.Id == id);
            if (levelReward == null)
            {
                return NotFound();
            }

            return View(levelReward);
        }

        // GET: LevelRewards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LevelRewards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GuildId,RequiredLevel,RoleId")] LevelReward levelReward)
        {
            if (ModelState.IsValid)
            {
                _context.Add(levelReward);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(levelReward);
        }

        // GET: LevelRewards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelReward = await _context.LevelReward.FindAsync(id);
            if (levelReward == null)
            {
                return NotFound();
            }
            return View(levelReward);
        }

        // POST: LevelRewards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GuildId,RequiredLevel,RoleId")] LevelReward levelReward)
        {
            if (id != levelReward.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(levelReward);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LevelRewardExists(levelReward.Id))
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
            return View(levelReward);
        }

        // GET: LevelRewards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var levelReward = await _context.LevelReward
                .FirstOrDefaultAsync(m => m.Id == id);
            if (levelReward == null)
            {
                return NotFound();
            }

            return View(levelReward);
        }

        // POST: LevelRewards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var levelReward = await _context.LevelReward.FindAsync(id);
            _context.LevelReward.Remove(levelReward);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LevelRewardExists(int id)
        {
            return _context.LevelReward.Any(e => e.Id == id);
        }
    }
}
