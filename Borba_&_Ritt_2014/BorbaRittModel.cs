using Base.Domain;
using Gurobi;

namespace Borba_and_Ritt_2014
{
    public class BorbaRittModel : Model
    {
        public Dictionary<(int, int, int), GRBVar> XVariables { get; private set; }
        public Dictionary<(int, int, int), GRBVar> DVariables { get; private set; }
        public Dictionary<(int, int), GRBVar> ZVariables { get; private set; }
        public Dictionary<int, GRBVar> CVariables { get; private set; }
        public int MaximumMeanCycleTime { get; private set; }
        public ConstraintController Controller { get; private set; }
        public long ExecutionTimeMs { get; private set; }

        public BorbaRittModel(GRBEnv env, int numberOfPeriods, Input instance, int maximumMeanCycleTime, ConstraintController controller) : base(env, numberOfPeriods, instance)
        {
            XVariables = new();
            DVariables = new();
            ZVariables = new();
            CVariables = new();
            MaximumMeanCycleTime = maximumMeanCycleTime;
            Controller = controller;
            BuildModel();
        }

        protected override void CreateVariables()
        {
            CreateZVariables();
            CreateXVariables();
            CreateDVariables();
            CreateCVariables();
        }

        protected override void DefineSense()
        {
            ModelSense = GRB.MAXIMIZE;
        }

        private void CreateXVariables()
        {
            foreach (int task in Instance.GetTasksList())
            {
                foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                {
                    foreach (int period in GetPeriodsList())
                    {
                        XVariables.Add((worker, task, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"X(w({worker})_i({task})_t({period}))"));
                    }
                }
            }
        }

        private void CreateDVariables()
        {
            foreach (int workerV in Instance.GetWorkersList())
            {
                foreach (int workerW in Instance.GetWorkersList())
                {
                    if (workerV == workerW)
                        continue;

                    foreach (int period in GetPeriodsList())
                    {
                        DVariables.Add((workerV, workerW, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"D(v({workerV})_w({workerW})_t({period}))"));
                    }
                }
            }
        }

        private void CreateZVariables()
        {
            foreach (int task in Instance.GetTasksList())
            {
                foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                {
                    ZVariables.Add((worker, task), AddVar(0d, 1d, 1d, GRB.BINARY, $"Z(w({worker})_i({task}))"));
                }
            }
        }

        private void CreateCVariables()
        {
            foreach (int period in GetPeriodsList())
            {
                CVariables.Add(period, AddVar(0d, MaximumMeanCycleTime * NumberOfPeriods, 0d, GRB.CONTINUOUS, $"C(t({period}))"));
            }
        }

        protected override void CreateConstraints()
        {
            CreateAllTasksMustBeExecutedConstraint(); // 1.34
            CreateImmediatePrecedenceConstraint(); // 1.35
            CreatePeriodCycleTimeConstraint(); // 1.36
            CreateSumOfCycleTimeConstraint(); // 1.37
            CreateAssociateDVariablesToXVariablesConstraint(); // 1.38
            CreateAvoidDVariablesInconsistencyConstraint(); // 1.39
            CreateLimitZVariablesConstraint(); // 1.40
            // Constraints 1.41, 1.42 and 1.43 are integrity constraints to define the variables x, d and z as binary
        }

        private void CreateAllTasksMustBeExecutedConstraint() // 1.34
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int task in Instance.GetTasksList())
                {
                    GRBLinExpr expression = new();
                    foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                    {
                        expression.AddTerm(1d, XVariables[(worker, task, period)]);
                    }
                    AddConstr(expression, GRB.EQUAL, 1d, $"AllTasksMustBeExecutedConstraint_i({task})_t({period})");
                }
            }
        }

        private void CreateImmediatePrecedenceConstraint() // 1.35
        {
            foreach (int workerU in Instance.GetWorkersList())
            {
                foreach (int workerV in Instance.GetWorkersList())
                {
                    if (workerU == workerV)
                        continue;

                    foreach (int workerW in Instance.GetWorkersList())
                    {
                        if (workerW == workerV || workerW == workerU)
                            continue;

                        foreach (int period in GetPeriodsList())
                        {
                            AddConstr(DVariables[(workerU, workerW, period)], GRB.GREATER_EQUAL,
                                DVariables[(workerU, workerV, period)] + DVariables[(workerV, workerW, period)] - 1,
                                $"ImmediatePrecedenceConstraint_u({workerU})_v({workerV})_w({workerW})_t({period})");
                        }
                    }
                }
            }
        }

        private void CreatePeriodCycleTimeConstraint() // 1.36
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int worker in Instance.GetWorkersList())
                {
                    GRBLinExpr expression = new();
                    foreach (int task in Instance.GetTasksExecutedByWorker(worker))
                    {
                        expression.AddTerm(Instance.GetTaskTime(task, worker)!.Value, XVariables[(worker, task, period)]);
                    }
                    AddConstr(expression, GRB.LESS_EQUAL, CVariables[period], $"PeriodCycleTimeConstraint_w({worker})_t({period})");
                }
            }
        }

        private void CreateSumOfCycleTimeConstraint() // 1.37
        {
            GRBLinExpr expression = new();
            foreach (int period in GetPeriodsList())
            {
                expression.AddTerm(1d, CVariables[period]);
            }
            AddConstr(expression, GRB.LESS_EQUAL, NumberOfPeriods * MaximumMeanCycleTime, $"SumOfCycleTimeConstraint");
        }

        private void CreateAssociateDVariablesToXVariablesConstraint() // 1.38
        {
            foreach (int task1 in Instance.GetTasksList())
            {
                foreach (int task2 in Instance.ImmediateFollowers[task1])
                {
                    foreach (int workerV in Instance.GetWorkersWhoCanExecuteTask(task1))
                    {
                        foreach (int workerW in Instance.GetWorkersWhoCanExecuteTask(task2))
                        {
                            if (workerV == workerW)
                                continue;

                            foreach (int period in GetPeriodsList())
                            {
                                AddConstr(DVariables[(workerV, workerW, period)], GRB.GREATER_EQUAL,
                                    XVariables[(workerV, task1, period)] + XVariables[(workerW, task2, period)] - 1,
                                    $"AssociateDVariablesToXVariablesConstraint_i({task1})_j({task2})_v({workerV})_w({workerW})_t({period})");
                            }
                        }
                    }
                }
            }
        }

        private void CreateAvoidDVariablesInconsistencyConstraint() // 1.39
        {
            foreach (int workerV in Instance.GetWorkersList())
            {
                foreach (int workerW in Instance.GetWorkersList())
                {
                    if (workerV <= workerW)
                        continue;

                    foreach (int period in GetPeriodsList())
                    {
                        AddConstr(DVariables[(workerV, workerW, period)] + DVariables[(workerW, workerV, period)], GRB.LESS_EQUAL, 1,
                            $"AvoidDVariablesInconsistencyConstraint_v({workerV})_w({workerW})_t({period})");
                    }
                }
            }
        }

        private void CreateLimitZVariablesConstraint() // 1.40
        {
            foreach (int task in Instance.GetTasksList())
            {
                foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                {
                    GRBLinExpr expression = new();
                    foreach (int period in GetPeriodsList())
                    {
                        expression.AddTerm(1d, XVariables[(worker, task, period)]);
                    }
                    AddConstr(ZVariables[(worker, task)], GRB.LESS_EQUAL, expression, $"LimitZVariablesConstraint_w({worker})_i({task})");
                }
            }
        }

        protected override void AddExtraConstraints()
        {
            if (Controller != ConstraintController.NoExtraConstraint)
            {
                if (Controller != ConstraintController.SecondExtraConstraint) { }
                CreateFirstExtraConstraint(); // 1.44
                if (Controller != ConstraintController.FirstExtraConstraint) { }
                CreateSecondExtraConstraint(); // 1.45
            }
        }

        private void CreateFirstExtraConstraint() // 1.44
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
                                AddConstr(XVariables[(worker, task2, period)], GRB.GREATER_EQUAL,
                                    XVariables[(worker, task1, period)] + XVariables[(worker, task3, period)] - 1,
                                    $"FirstExtraConstraint_i({task1})_j({task2})_k({task3})_w({worker})_t({period})");
                            }
                        }
                    }
                }
            }
        }

        private void CreateSecondExtraConstraint() // 1.45
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
                                AddConstr(XVariables[(worker, task1, period)] + XVariables[(worker, task3, period)], GRB.LESS_EQUAL,
                                    1d, $"SecondExtraConstraint_i({task1})_j({task2})_k({task3})_w({worker})_t({period})");
                            }
                        }
                    }
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
            Dictionary<(int, int), int> cycleTimes = new();
            Solution = new Solution(Instance.NumberOfTasks, Instance.Workers, NumberOfPeriods, numberOfDistinctTasksExecuted, ExecutionTimeMs);

            foreach (int worker in Instance.GetWorkersList())
            {
                foreach (int period in GetPeriodsList())
                {
                    int station = DVariables.Keys.Where(x => x.Item2 == worker && x.Item3 == period && DVariables[x].X == 1d).Count();
                    if (!cycleTimes.ContainsKey((station, period)))
                        cycleTimes.Add((station, period), 0);

                    List<int> executedTasks = new();
                    foreach (int task in Instance.GetTasksExecutedByWorker(worker))
                    {
                        if (XVariables[(worker, task, period)].X == 1d)
                            executedTasks.Add(task);
                    }
                    if (executedTasks.Any())
                        Solution.AddAssignment(period, station, worker, executedTasks);

                    executedTasks.ForEach(x => cycleTimes[(station, period)] += Instance.GetTaskTime(x, worker)!.Value);
                }
            }

            //double meanCycleTime = CVariables.Values.Select(x => x.X).Average();
            double meanCycleTime = cycleTimes.Values.Average();
            Solution.SetMeanCycleTime(meanCycleTime);

            foreach (int worker in Instance.GetWorkersList())
            {
                List<int> executedTasks = new();
                foreach (int period in GetPeriodsList())
                {
                    int station = DVariables.Keys.Where(x => x.Item2 == worker && x.Item3 == period && DVariables[x].X == 1d).Count();
                    List<int> newTasks = new();
                    foreach (int task in Instance.GetTasksExecutedByWorker(worker))
                    {
                        if (XVariables[(worker, task, period)].X == 1d)
                        {
                            if (!executedTasks.Contains(task))
                                newTasks.Add(task);
                        }
                    }

                    executedTasks.AddRange(newTasks);
                    Solution.AddNewTasksExecutedByWorkerOnEachPeriod(period, worker, newTasks);
                }
            }
        }
    }
}
