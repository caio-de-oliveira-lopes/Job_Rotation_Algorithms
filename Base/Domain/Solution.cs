namespace Base.Domain
{
    public class Solution
    {
        public int NumberOfTasks { get; private set; }
        public int NumberOfWorkers { get; private set; }
        public int NumberOfPeriods { get; private set; }
        public int NumberOfDistinctTasksExecuted { get; private set; }
        public double MeanCycleTime { get; private set; }
        public long ExecutionTimeMs { get; private set; }
        // Key is (period, station) and Value is (worker, ListOfTasks) 
        public Dictionary<string, Dictionary<string, Dictionary<string, List<int>>>> Assignment { get; private set; }
        // Key is (period, worker) and Value is the number of new tasks executed by that worker on a said period
        public Dictionary<string, Dictionary<string, List<int>>> NewTasksExecutedByWorkerOnEachPeriod { get; private set; }

        public Solution(int numberOfTasks, int numberOfWorkers, int numberOfPeriods, int numberOfDistinctTasksExecuted, double meanCycleTime, long executionTimeMs)
        {
            NumberOfTasks = numberOfTasks;
            NumberOfWorkers = numberOfWorkers;
            NumberOfPeriods = numberOfPeriods;
            NumberOfDistinctTasksExecuted = numberOfDistinctTasksExecuted;
            MeanCycleTime = meanCycleTime;
            ExecutionTimeMs = executionTimeMs;
            Assignment = new();
            NewTasksExecutedByWorkerOnEachPeriod = new();
        }

        public void AddAssignment(int period, int station, int worker, List<int> tasks) 
        {
            Assignment.TryAdd($"Period {period}", new Dictionary<string, Dictionary<string, List<int>>>());
            Assignment[$"Period {period}"].TryAdd($"Station {station}", new Dictionary<string, List<int>>());
            Assignment[$"Period {period}"][$"Station {station}"].TryAdd($"Worker {worker}", tasks);
        }

        public void AddNewTasksExecutedByWorkerOnEachPeriod(int period, int worker, List<int> newTasks)
        {
            NewTasksExecutedByWorkerOnEachPeriod.TryAdd($"Period {period}", new Dictionary<string, List<int>>());
            NewTasksExecutedByWorkerOnEachPeriod[$"Period {period}"].TryAdd($"Worker {worker}", newTasks);
        }

        public int GetNumberOfNewTasksExecutedByWorkerOnPeriod(int period, int worker)
        {
            if (NewTasksExecutedByWorkerOnEachPeriod.TryGetValue($"Period {period}", out var workerDict) && workerDict.TryGetValue($"Worker {worker}", out var tasks))
                return tasks.Count;
            else
                return 0;
        }
    }
}