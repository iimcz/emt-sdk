using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Yort.Ntp;

namespace emt_sdk.Events.NtpSync
{
    public class NtpScheduler
    {
        private readonly NtpClient _client;
        private readonly Stopwatch _stopwatch;
        private readonly List<NtpAction> _actions = new List<NtpAction>();

        private DateTime _ntpSync = DateTime.UtcNow;

        public DateTime SynchronizedTime => _ntpSync + _stopwatch.Elapsed;

        /// <summary>
        /// Creates an NTP Scheduler with europe.pool.ntp.org as server
        /// </summary>
        public NtpScheduler()
        {
            _client = new NtpClient(KnownNtpServers.Europe);
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Creates an NTP scheduler with a custom server
        /// </summary>
        /// <param name="host"></param>
        public NtpScheduler(string host)
        {
            _client = new NtpClient(host);
            _stopwatch = Stopwatch.StartNew();
        }

        public async Task Resync()
        {
            var currentTime = await _client.RequestTimeAsync();

            _ntpSync = currentTime.NtpTime;
            _stopwatch.Restart();
        }

        public void ScheduleAction(NtpAction action)
        {
            if (action.ScheduledTime < SynchronizedTime) throw new InvalidOperationException("Cannot scheduled a task that already happened");
            _actions.Add(action);
        }

        public void RemoveAction(string name)
        {
            var removedAction = _actions.First(a => a.Name == name);
            _actions.Remove(removedAction);
        }

        // This is because we need to run everything on the main thread in Unity
        /// <summary>
        /// Checks and executes any due actions. Must be called manually.
        /// </summary>
        public void RunActions()
        {
            var dueActions = _actions.Where(a => a.IsDue(SynchronizedTime)).ToList();

            foreach (var action in dueActions)
            {
                action.Action.Invoke();
                _actions.Remove(action);
            }
        }
    }
}
