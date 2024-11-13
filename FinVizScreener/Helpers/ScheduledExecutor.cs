using Microsoft.Extensions.Logging;

namespace FinVizScreener.Helpers
{
    internal class ScheduledExecutor
    {
        public static async Task RunScheduledTask(TimeSpan timeForExecution, CancellationToken ct,
            Action task, ILogger? logger=null)
        {
            await PauseUntil(ct, timeForExecution, logger);
            task.Invoke();
        }

        private static async Task PauseUntil(CancellationToken ct, TimeSpan targetTime, ILogger? logger = null)
        {
            DateTime now = DateTime.Now;
            DateTime targetDateTime = DateTime.Today.Add(targetTime);

            if (now > targetDateTime)
            {
                targetDateTime = targetDateTime.AddDays(1);
            }
            TimeSpan delay = targetDateTime - now;
            logger?.Log(LogLevel.Information, $"ScheduledExecutor: " +
                            $"task execution postponed to {targetDateTime}. " +
                            $"Waiting {delay.TotalSeconds} seconds.");
            await Task.Delay(delay, ct);
        }
    }
}
