﻿using Base.DataManagers;

namespace Base.Domain
{
    public class Output : BaseFile
    {
        private static int InstanceCounter = 0;
        public int Id { get; private set; }
        public Solution? Solution { get; private set; }

        public Output(string fileDirectory, string fileName, int maximumMeanCycleTime, Model.ModelType modelType, Model.ConstraintController constraintController, int numberOfPeriods)
            : base(fileDirectory, $"output-{fileName}-c{maximumMeanCycleTime}-{modelType}-{constraintController}-{numberOfPeriods}", ".json")
        {
            Id = InstanceCounter++;
        }

        public void SetSolution(Solution solution)
        {
            Solution = solution;
        }

        public override void Write(params object[] objs)
        {
            int originalMaximumMeanCycleTime = (int)objs[0];
            int maximumMeanCycleTime = (int)objs[1];
            double percentage = (double)objs[2];

            if (Solution == null) return;

            Dictionary<string, object?> data = new()
            {
                { nameof(Solution.NumberOfTasks), Solution.NumberOfTasks },
                { nameof(Solution.NumberOfWorkers), Solution.NumberOfWorkers },
                { nameof(Solution.NumberOfPeriods), Solution.NumberOfPeriods },
                { "OriginalMaximumMeanCycleTime", originalMaximumMeanCycleTime },
                { "MaximumMeanCycleTime", maximumMeanCycleTime },
                { "PercentageAppliedOverOriginalMaximumMeanCycleTime", percentage },
                { nameof(Solution.MeanCycleTime), Solution.MeanCycleTime },
                { nameof(Solution.ExecutionTimeMs), Solution.ExecutionTimeMs },
                { "OF", Solution.NumberOfDistinctTasksExecuted },
                { nameof(Solution.Assignment), Solution.Assignment },
                { nameof(Solution.NewTasksExecutedByWorkerOnEachPeriod), Solution.NewTasksExecutedByWorkerOnEachPeriod }
            };

            Writer.WriteOutput(this, data);
        }
    }
}