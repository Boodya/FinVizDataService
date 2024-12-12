using Microsoft.Extensions.Logging;

namespace FinVizScreener.Helpers
{
    public class ScheduledExecutor
    {
        public static async Task ScheduleTaskExecution(
            TimeSpan executionTime,
            CancellationToken ct,
            Func<Task> task,
            bool isSyncOnStart = false,
            ILogger? logger = null)
        {
            if (isSyncOnStart)
            {
                logger?.Log(LogLevel.Information, $"ScheduledExecutor: " +
                            $"Executing task on start");
                await task();
            }
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var nowDate = DateTime.UtcNow;
                    var nowExecDate = SetTime(nowDate, executionTime);
                    var nextExecDate = SetTime(nowDate.AddDays(1), executionTime);
                    var delay = nextExecDate - nowDate;
                    logger?.Log(LogLevel.Information, $"ScheduledExecutor: " +
                            $"Execution schedulled to {nextExecDate} UTC. Waiting {delay}");
                    await Task.Delay(delay, ct);
                    logger?.Log(LogLevel.Information, $"ScheduledExecutor: " +
                            $"Executing task");
                    await task();
                }
                catch (OperationCanceledException)
                {
                    logger?.LogInformation("ScheduledExecutor: execution cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "ScheduledExecutor: issue with task execution.");
                }
            }
        }

        private static DateTime SetTime(DateTime date, TimeSpan time)
        {
            return new DateTime(
                date.Year,
                date.Month,
                date.Day,
                time.Hours,
                time.Minutes,
                time.Seconds,
                date.Kind
            );
        }
    }
}
