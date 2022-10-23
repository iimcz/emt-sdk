using System;

namespace emt_sdk.Events.NtpSync
{
    public class NtpAction
    {
        public DateTime ScheduledTime { get; }
        public Action Action { get; }
        public string Name { get; }

        public NtpAction(DateTime scheduledTime, Action action, string name)
        {
            ScheduledTime = scheduledTime;
            Action = action;
            Name = name;
        }

        public bool IsDue(DateTime time)
        {
            return time >= ScheduledTime;
        }
    }
}
