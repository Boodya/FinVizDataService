using Microsoft.Extensions.Logging;

namespace FinVizScreener.Helpers
{
    public static class ScheduledExecutor
    {
        public static async Task ScheduleTaskExecution<T>(
            TimeSpan executionTime,
            CancellationToken ct,
            Func<Task> task,
            bool isSyncOnStart = false,
            ILogger? logger = null)
        {
            var executorType = typeof(T).Name;
            if (isSyncOnStart)
            {
                logger?.Log(LogLevel.Information, $"{executorType} ScheduledExecutor: " +
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
                    logger?.Log(LogLevel.Information, $"{executorType} ScheduledExecutor: " +
                            $"Execution schedulled to {nextExecDate} UTC. Waiting {delay}");
                    await Task.Delay(delay, ct);
                    logger?.Log(LogLevel.Information, $"{executorType} ScheduledExecutor: " +
                            $"Executing task");
                    await task();
                }
                catch (OperationCanceledException)
                {
                    logger?.LogInformation($"{executorType} ScheduledExecutor: " +
                        $"execution cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, $"{executorType} ScheduledExecutor: " +
                        $"issue with task execution.");
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
