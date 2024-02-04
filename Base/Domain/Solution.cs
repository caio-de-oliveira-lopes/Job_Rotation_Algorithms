namespace Base.Domain
{
    public class Solution
    {
        public int NumberOfTasks { get; private set; }
        public int NumberOfWorkers { get; private set; }
        public int NumberOfPeriods { get; private set; }
        public int NumberOfDistinctTasksExecuted { get; private set; }
        public int MeanCycleTime { get; private set; }
        public long ExecutionTimeMs { get; private set; }
        // Key is (period, station) and Value is (worker, ListOfTasks) 
        public Dictionary<(int, int), (int, List<int>)> Assignment { get; private set; }
        // Key is (period, worker) and Value is the number of new tasks executed by that worker on a said period
        public Dictionary<(int, int), List<int>> NewTasksExecutedByWorkerOnEachPeriod { get; private set; }

        public Solution(int numberOfTasks, int numberOfWorkers, int numberOfPeriods, int numberOfDistinctTasksExecuted, int meanCycleTime, long executionTimeMs)
        {
            NumberOfTasks = numberOfTasks;
            NumberOfWorkers = numberOfWorkers;
            NumberOfPeriods = numberOfPeriods;
            NumberOfDistinctTasksExecuted = numberOfDistinctTasksExecuted;
            MeanCycleTime = meanCycleTime;
            Assignment = new();
            NewTasksExecutedByWorkerOnEachPeriod = new();
            ExecutionTimeMs = executionTimeMs;
        }

        public void AddAssignment(int period, int station, int worker, List<int> tasks) 
        {
            Assignment.Add((period, station), (worker, tasks));
        }

        public void AddNewTasksExecutedByWorkerOnEachPeriod(int period, int worker, List<int> newTasks)
        {
            NewTasksExecutedByWorkerOnEachPeriod.Add((period, worker), newTasks);
        }

        public int GetNumberOfNewTasksExecutedByWorkerOnPeriod(int period, int worker)
        {
            if (NewTasksExecutedByWorkerOnEachPeriod.TryGetValue((period, worker), out List<int>? tasks))
                return tasks.Count;
            else
                return 0;
        }
    }
}