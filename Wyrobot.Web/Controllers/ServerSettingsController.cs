using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wyrobot.Web.Models;
using Wyrobot.Web.Data;
using Wyrobot.Core;

namespace Wyrobot.Web.Controllers
{
    public class ServerSettingsController : Controller
    {
        private readonly WyrobotWebContext _context;

        public ServerSettingsController(WyrobotWebContext context)
        {
            _context = context;
        }

        // GET: ServerSettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServerSettings.ToListAsync());
        }

        // GET: ServerSettings/Details/5
        public async Task<IActionResult> Details(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serverSettings = await _context.ServerSettings
                .FirstOrDefaultAsync(m => m.GuildId == id.ToUInt64());
            if (serverSettings == null)
            {
                return NotFound();
            }

            return View(serverSettings);
        }

        // GET: ServerSettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServerSettings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GuildId,GuildName,MuteRoleId,ModerationRoles,BannedWords,CapsPercentage,AutoModerationEnabled,MuteAfter3Warn,Enabled,WelcomeChannelId,WelcomeMessage,LogsEnabled,LogsChannelId,LogMessages,LogPunishments,LogInvites,LogVoiceState,LogChannels,LogUsers,LogRoles,LogServer,LevelingEnabled,Multiplier,LevelingMessage,LoungesEnabled,CatCmdEnabled,DogCmdEnabled")] ServerSettings serverSettings)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serverSettings);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(serverSettings);
        }

        // GET: ServerSettings/Edit/5
        public async Task<IActionResult> Edit(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serverSettings = await _context.ServerSettings.FindAsync(id);
            if (serverSettings == null)
            {
                return NotFound();
            }
            return View(serverSettings);
        }

        // POST: ServerSettings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ulong id, [Bind("Id,GuildId,GuildName,MuteRoleId,ModerationRoles,BannedWords,CapsPercentage,AutoModerationEnabled,MuteAfter3Warn,Enabled,WelcomeChannelId,WelcomeMessage,LogsEnabled,LogsChannelId,LogMessages,LogPunishments,LogInvites,LogVoiceState,LogChannels,LogUsers,LogRoles,LogServer,LevelingEnabled,Multiplier,LevelingMessage,LoungesEnabled,CatCmdEnabled,DogCmdEnabled")] ServerSettings serverSettings)
        {
            if (id.ToUInt64() != serverSettings.GuildId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serverSettings);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServerSettingsExists(serverSettings.GuildId))
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
            return View(serverSettings);
        }

        // GET: ServerSettings/Delete/5
        public async Task<IActionResult> Delete(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serverSettings = await _context.ServerSettings
                .FirstOrDefaultAsync(m => m.GuildId == id.ToUInt64());
            if (serverSettings == null)
            {
                return NotFound();
            }

            return View(serverSettings);
        }

        // POST: ServerSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(ulong id)
        {
            var serverSettings = await _context.ServerSettings.FindAsync(id);
            _context.ServerSettings.Remove(serverSettings);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServerSettingsExists(ulong id)
        {
            return _context.ServerSettings.Any(e => e.GuildId == id.ToUInt64());
        }
    }
}
