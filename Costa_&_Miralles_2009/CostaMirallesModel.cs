using Base.Domain;
using Gurobi;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Costa_and_Miralles_2009
{
    public class CostaMirallesModel : Model
    {
        public Dictionary<(int, int, int, int), GRBVar> XVariables { get; private set; }
        public Dictionary<(int, int, int), GRBVar> YVariables { get; private set; }
        public Dictionary<(int, int), GRBVar> ZVariables { get; private set; }
        public Dictionary<int, GRBVar> CVariables { get; private set; }
        public int MaximumMeanCycleTime { get; private set; }
        public ConstraintController Controller { get; private set; }
        public long ExecutionTimeMs { get; private set; }

        public CostaMirallesModel(GRBEnv env, int numberOfPeriods, Input instance, int maximumMeanCycleTime, ConstraintController controller) : base(env, numberOfPeriods, instance)
        {
            XVariables = new();
            YVariables = new();
            ZVariables = new();
            CVariables = new();
            MaximumMeanCycleTime = maximumMeanCycleTime;
            Controller = controller;
            BuildModel();
        }

        protected override void CreateVariables()
        {
            CreateXVariables();
            CreateYVariables();
            CreateZVariables();
            CreateCVariables();
        }

        protected override void DefineSense()
        {
            ModelSense = GRB.MAXIMIZE;
        }

        private void CreateXVariables()
        {
            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int worker in Instance.GetWorkersList())
                {
                    foreach (int task in Instance.GetTasksList())
                    {
                        foreach (int period in GetPeriodsList())
                        {
                            XVariables.Add((station, worker, task, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"X(s({station})_w({worker})_i({task})_t({period}))"));
                        }
                    }
                }
            }
        }

        private void CreateYVariables()
        {
            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int worker in Instance.GetWorkersList())
                {
                    foreach (int period in GetPeriodsList())
                    {
                        YVariables.Add((station, worker, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"Y(s({station})_w({worker})_t({period}))"));
                    }
                }
            }
        }

        private void CreateZVariables()
        {
            foreach (int worker in Instance.GetWorkersList())
            {
                foreach (int task in Instance.GetTasksList())
                {
                    ZVariables.Add((worker, task), AddVar(0d, 1d, 1d, GRB.BINARY, $"Z(w({worker})_i({task}))"));
                }
            }            
        }

        private void CreateCVariables()
        {
            foreach (int period in GetPeriodsList())
            {
                CVariables.Add(period, AddVar(0d, MaximumMeanCycleTime * NumberOfPeriods * 2, 0d, GRB.CONTINUOUS, $"C(t({period}))"));
            }
        }

        protected override void CreateConstraints()
        {
            CreateTaskAssignmentToOneWorkerConstraint(); // 1.2
            CreateWorkerAssignmentConstraint(); // 1.3
            CreateStationAssignmentConstraint(); // 1.4
            CreateImmediatePrecedenceConstraint(); // 1.5
            CreatePeriodCycleTimeConstraint(); // 1.6
            CreateSumOfCycleTimeConstraint(); // 1.7
            CreateLimitXVariablesConstraint(); // 1.8
            CreateLimitZVariablesConstraint(); // 1.9
            // Constraints 1.10, 1.11 and 1.12 are integrity constraints to define the variables x, y and z as binary
        }

        private void CreateLimitZVariablesConstraint() // 1.9
        {
            foreach (int task in Instance.GetTasksList())
            {
                foreach(int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                {
                    GRBLinExpr expression = new();
                    foreach (int period in GetPeriodsList())
                    {
                        foreach (int station in Instance.GetWorkersList())
                        {
                            expression.AddTerm(1d, XVariables[(station, worker, task, period)]);
                        }
                    }
                    AddConstr(ZVariables[(worker, task)], GRB.LESS_EQUAL, expression, $"LimitZVariablesConstraint_w({worker})_i({task})");
                }
            }
        }

        private void CreateLimitXVariablesConstraint() // 1.8
        {
            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int worker in Instance.GetWorkersList())
                {
                    foreach (int period in GetPeriodsList())
                    {
                        GRBLinExpr expression = new();
                        foreach (int task in Instance.GetTasksList().Where(i => Instance.GetWorkersWhoCanExecuteTask(i).Contains(worker)))
                        {
                            expression.AddTerm(1d, XVariables[(station, worker, task, period)]);
                        }
                        AddConstr(expression, GRB.LESS_EQUAL, Instance.NumberOfTasks * YVariables[(station, worker, period)], $"LimitXVariablesConstraint_s({station})_w({worker})_t({period})");
                    }
                }
            }
        }

        private void CreateSumOfCycleTimeConstraint() // 1.7
        {
            GRBLinExpr expression = new();
            foreach (int period in GetPeriodsList())
            {
                expression.AddTerm(1d, CVariables[period]);
            }
            AddConstr(expression, GRB.LESS_EQUAL, NumberOfPeriods * MaximumMeanCycleTime, $"SumOfCycleTimeConstraint");
        }

        private void CreatePeriodCycleTimeConstraint() // 1.6
        {
            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int period in GetPeriodsList())
                {
                    GRBLinExpr expression = new();
                    foreach (int task in Instance.GetTasksList())
                    {
                        foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                        {
                            expression.AddTerm(Instance.GetTaskTime(task, worker)!.Value, XVariables[(station, worker, task, period)]);
                        }
                    }
                    AddConstr(expression, GRB.LESS_EQUAL, CVariables[period], $"PeriodCycleTimeConstraint_s({station})_t({period})");
                }
            }
        }

        private void CreateImmediatePrecedenceConstraint() // 1.5
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int stationK in Instance.GetWorkersList())
                {
                    if (stationK == Instance.GetWorkersList().First())
                        continue;

                    foreach (int task1 in Instance.GetTasksList())
                    {
                        foreach (int task2 in Instance.ImmediateFollowers[task1])
                        {
                            var workers = Instance.GetWorkersExecutionIntersection(new List<int>() { task1, task2 });
                            foreach (int worker in workers)
                            {
                                GRBLinExpr expression1 = new();
                                GRBLinExpr expression2 = new();
                                foreach (int stationS in Instance.GetWorkersList().Where(s => s >= stationK))
                                {
                                    expression1.AddTerm(stationS, XVariables[(stationS, worker, task1, period)]);
                                    expression2.AddTerm(stationS, XVariables[(stationS, worker, task2, period)]);
                                }
                                AddConstr(expression1, GRB.LESS_EQUAL, expression2, $"ImmediatePrecedenceConstraint_i({task1})_j({task2})_k({stationK})_w({worker})_t({period})");
                            }
                        }
                    }
                }
            }
        }

        private void CreateTaskAssignmentToOneWorkerConstraint() // 1.2
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int task in Instance.GetTasksList())
                {
                    GRBLinExpr expression = new();
                    foreach (int station in Instance.GetWorkersList())
                    {
                        foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                        {
                            expression.AddTerm(1d, XVariables[(station, worker, task, period)]);
                        }
                    }
                    AddConstr(expression, GRB.EQUAL, 1d, $"TaskAssignmentToOneWorkerConstraint_i({task})_t({period})");
                }
            }
        }

        protected override void AddExtraConstraints()
        {
            if (Controller != ConstraintController.NoExtraConstraint)
            {
                if (Controller != ConstraintController.SecondExtraConstraint) { }
                    CreateFirstExtraConstraint(); // 1.13
                if (Controller != ConstraintController.FirstExtraConstraint) { }
                    CreateSecondExtraConstraint(); // 1.14
            }
        }

        private void CreateSecondExtraConstraint() // 1.14
        {
            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int task1 in Instance.GetTasksList())
                {
                    foreach (int task2 in Instance.Followers[task1])
                    {
                        foreach (int task3 in Instance.Followers[task2])
                        {
                            foreach (int worker in Instance.GetWorkersExecutionIntersection(new List<int>() { task1, task3 }, new List<int>() { task2 }))
                            {
                                foreach (int period in GetPeriodsList())
                                {
                                    AddConstr(XVariables[(station, worker, task1, period)] + XVariables[(station, worker, task3, period)], GRB.LESS_EQUAL,
                                        1d, $"SecondExtraConstraint_s({station})_i({task1})_j({task2})_k({task3})_w({worker})_t({period})");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateFirstExtraConstraint() // 1.13
        {
            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int task1 in Instance.GetTasksList())
                {
                    foreach (int task2 in Instance.Followers[task1])
                    {
                        foreach (int task3 in Instance.Followers[task2])
                        {
                            foreach (int worker in Instance.GetWorkersExecutionIntersection(new List<int>() { task1, task2, task3 }))
                            {
                                foreach (int period in GetPeriodsList())
                                {
                                    AddConstr(XVariables[(station, worker, task2, period)], GRB.GREATER_EQUAL,
                                        XVariables[(station, worker, task1, period)] + XVariables[(station, worker, task3, period)] - 1,
                                        $"FirstExtraConstraint_s({station})_i({task1})_j({task2})_k({task3})_w({worker})_t({period})");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateWorkerAssignmentConstraint() // 1.3
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int worker in Instance.GetWorkersList())
                {
                    GRBLinExpr expression = new();
                    foreach (int station in Instance.GetWorkersList())
                    {
                        expression.AddTerm(1d, YVariables[(station, worker, period)]);
                    }
                    AddConstr(expression, GRB.LESS_EQUAL, 1d, $"WorkerAssignmentOnlyToOneStationPerPeriod_w({worker})_t({period})");
                }
            }
        }

        private void CreateStationAssignmentConstraint() // 1.4
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int station in Instance.GetWorkersList())
                {
                    GRBLinExpr expression = new();
                    foreach (int worker in Instance.GetWorkersList())
                    {
                        expression.AddTerm(1d, YVariables[(station, worker, period)]);
                    }
                    AddConstr(expression, GRB.LESS_EQUAL, 1d, $"StationAssignmentOnlyToOneWorkerPerPeriod_s({station})_t({period})");
                }
            }
        }

        public override void Run()
        {
            System.Diagnostics.Stopwatch watch = new();
            watch.Start();
            Optimize();
            watch.Stop();
            ExecutionTimeMs = watch.ElapsedMilliseconds;
        }

        protected override void CompileSolution()
        {
            int numberOfDistinctTasksExecuted = (int)ZVariables.Values.Select(x => x.X).Sum();
            double meanCycleTime = CVariables.Values.Select(x => x.X).Average();
            Solution = new Solution(Instance.NumberOfTasks, Instance.Workers, NumberOfPeriods, numberOfDistinctTasksExecuted, meanCycleTime, ExecutionTimeMs);

            foreach (int station in Instance.GetWorkersList())
            {
                foreach (int worker in Instance.GetWorkersList())
                {
                    foreach (int period in GetPeriodsList())
                    {
                        List<int> executedTasks = new();
                        foreach (int task in Instance.GetTasksList())
                        {
                            if (XVariables[(station, worker, task, period)].X == 1d)
                                executedTasks.Add(task);
                        }
                        if (executedTasks.Any())
                            Solution.AddAssignment(period, station, worker, executedTasks);
                    }
                }
            }

            foreach (int worker in Instance.GetWorkersList())
            {
                List<int> executedTasks = new();
                foreach (int period in GetPeriodsList())
                {
                    List<int> newTasks = new();
                    foreach (int station in Instance.GetWorkersList())
                    {
                        foreach (int task in Instance.GetTasksList())
                        {
                            if (XVariables[(station, worker, task, period)].X == 1d)
                            {
                                if (!executedTasks.Contains(task))
                                    newTasks.Add(task);
                            }
                        }
                    }
                    executedTasks.AddRange(newTasks);
                    Solution.AddNewTasksExecutedByWorkerOnEachPeriod(period, worker, newTasks);
                }
            }
        }

        public override void WriteSolution(Output output)
        {
            CompileSolution();
            if (HasSolution())
                output.SetSolution(Solution!);
        }
    }
}
