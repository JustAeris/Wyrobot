using System.Threading.Tasks;
using System.Timers;
using static Wyrobot.Core.Scheduler.Event;

namespace Wyrobot.Core.Scheduler
{
    public static class Setup
    {
        public static async Task SetupAsync()
        {
            var timer = new Timer {Interval = 60000, AutoReset = true};

            timer.Elapsed += TimerOnElapsed;
            
            timer.Start();
            
            await Task.Delay(-1);
        }
    }
}