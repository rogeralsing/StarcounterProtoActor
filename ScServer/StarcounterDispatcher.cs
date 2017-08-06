using System;
using System.Threading.Tasks;
using Proto.Mailbox;
using Starcounter;

namespace ScServer
{
    public class StarcounterDispatcher : IDispatcher
    {
        public void Schedule(Func<Task> runner)
        {
            void Action() => runner();

            Scheduling.ScheduleTask(Action);
        }

        public int Throughput { get; set; } = 10;
    }
}