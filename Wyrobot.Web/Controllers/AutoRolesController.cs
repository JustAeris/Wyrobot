using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wyrobot.Web.Data;
using Wyrobot.Web.Models;

namespace Wyrobot.Web.Controllers
{
    public class AutoRolesController : Controller
    {
        private readonly WyrobotWebContext _context;

        public AutoRolesController(WyrobotWebContext context)
        {
            _context = context;
        }

        // GET: AutoRoles
        public async Task<IActionResult> Index()
        {
            var list = await _context.AutoRole.ToListAsync();

            foreach (var autoRole in list)
            {
                var guild = await Program.DiscordClient.GetGuildAsync(autoRole.GuildId, false);
                var role = guild.GetRole(autoRole.RoleId);
                autoRole.RoleName = role.Name;
            }
            
            return View(list);
        }

        // GET: AutoRoles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autoRole = await _context.AutoRole
                .FirstOrDefaultAsync(m => m.Id == id);
            if (autoRole == null)
            {
                return NotFound();
            }

            return View(autoRole);
        }

        // GET: AutoRoles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AutoRoles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GuildId,RoleId")] AutoRole autoRole)
        {
            if (ModelState.IsValid)
            {
                _context.Add(autoRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(autoRole);
        }

        // GET: AutoRoles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autoRole = await _context.AutoRole.FindAsync(id);
            if (autoRole == null)
            {
                return NotFound();
            }
            return View(autoRole);
        }

        // POST: AutoRoles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GuildId,RoleId")] AutoRole autoRole)
        {
            if (id != autoRole.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(autoRole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AutoRoleExists(autoRole.Id))
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
            return View(autoRole);
        }

        // GET: AutoRoles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var autoRole = await _context.AutoRole
                .FirstOrDefaultAsync(m => m.Id == id);
            if (autoRole == null)
            {
                return NotFound();
            }

            return View(autoRole);
        }

        // POST: AutoRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var autoRole = await _context.AutoRole.FindAsync(id);
            _context.AutoRole.Remove(autoRole);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AutoRoleExists(int id)
        {
            return _context.AutoRole.Any(e => e.Id == id);
        }
    }
}
