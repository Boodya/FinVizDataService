using Microsoft.Extensions.Logging;

namespace FinVizScreener.Helpers
{
    internal class ScheduledExecutor
    {
        public static async Task RunScheduledTask(TimeSpan timeForExecution, CancellationToken ct,
            Action task, ILogger? logger=null)
        {
            await PauseUntil(ct, timeForExecution, logger);
            logger?.Log(LogLevel.Information, $"ScheduledExecutor: " +
                            $"task execution starting.");
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

            logger?.Log(LogLevel.Information, $"ScheduledExecutor: Task execution postponed to {targetDateTime}.");

            while (true)
            {
                now = DateTime.Now;
                if (now >= targetDateTime)
                {
                    logger?.Log(LogLevel.Information, "ScheduledExecutor: Time has reached the target. Proceeding with task execution.");
                    break;
                }

                TimeSpan remainingTime = targetDateTime - now;
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), ct);
                }
                catch (TaskCanceledException)
                {
                    logger?.Log(LogLevel.Warning, "ScheduledExecutor: Task delay was canceled.");
                    throw;
                }
            }
        }
    }
}
