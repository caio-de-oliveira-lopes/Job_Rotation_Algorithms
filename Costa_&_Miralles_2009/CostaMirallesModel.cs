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
        public ConstraintController Controller { get; private set; }

        public CostaMirallesModel(GRBEnv env, int numberOfPeriods, Input instance, ConstraintController controller) : base(env, numberOfPeriods, instance)
        {
            XVariables = new();
            YVariables = new();
            ZVariables = new();
            Controller = controller;
            BuildModel();
        }

        protected override void CreateVariables()
        {
            CreateXVariables();
            CreateYVariables();
            CreateZVariables();
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
                    ZVariables.Add((worker, task), AddVar(0d, 1d, 1d, GRB.BINARY, $"Z(w({worker})_t({task}))"));
                }
            }            
        }

        protected override void CreateConstraints()
        {
            CreateWorkerAssignmentToAtLeastOneTaskConstraint(); // 1.2
            CreateStationAssignmentToAtLeastOneWorkerConstraint(); // 1.3
            CreateWorkerAssignmentConstraint(); // 1.4
            CreateStationAssignmentConstraint(); // 1.5
            CreateImmediatePrecedenceConstraint(); // 1.6
        }

        private void CreateImmediatePrecedenceConstraint()
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int task1 in Instance.GetTasksList())
                {
                    GRBLinExpr expression1 = new();
                    foreach (int station in Instance.GetWorkersList())
                    {
                        foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task1))
                        {
                            expression1.AddTerm(station, XVariables[(station, worker, task1, period)]);
                        }
                    }

                    foreach (int task2 in Instance.ImmediateFollowers[task1])
                    {
                        GRBLinExpr expression2 = new();
                        foreach (int station in Instance.GetWorkersList())
                        {
                            foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task2))
                            {
                                expression2.AddTerm(station, XVariables[(station, worker, task2, period)]);
                            }
                        }
                        AddConstr(expression1, GRB.LESS_EQUAL, expression2, $"ImmediatePrecedenceConstraint_i({task1})_j({task2})_t({period})");
                    }
                }
            }
        }

        private void CreateWorkerAssignmentToAtLeastOneTaskConstraint()
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
                    AddConstr(expression, GRB.GREATER_EQUAL, 1d, $"WorkerAssignmentToAtLeastOneTask_i({task})_t({period})");
                }
            }
        }

        private void CreateStationAssignmentToAtLeastOneWorkerConstraint()
        {
            foreach (int period in GetPeriodsList())
            {
                foreach (int station in Instance.GetWorkersList())
                {
                    GRBLinExpr expression = new();
                    foreach (int task in Instance.GetTasksList())
                    {
                        foreach (int worker in Instance.GetWorkersWhoCanExecuteTask(task))
                        {
                            expression.AddTerm(1d, XVariables[(station, worker, task, period)]);
                        }
                    }
                    AddConstr(expression, GRB.GREATER_EQUAL, 1d, $"StationAssignmentToAtLeastOneWorker_s({station})_t({period})");
                }
            }
        }

        protected override void AddExtraConstraints()
        {
            if (Controller != ConstraintController.NoExtraConstraint)
            {
                if (Controller != ConstraintController.SecondExtraConstraint) { }
                    //CreateFirstExtraConstraint(); // 1.14
                if (Controller != ConstraintController.FirstExtraConstraint) { }
                    //CreateSecondExtraConstraint(); // 1.15
            }
        }

        private void CreateWorkerAssignmentConstraint()
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
                    AddConstr(expression, GRB.EQUAL, 1d, $"WorkerAssignmentOnlyToOneStationPerPeriod_w({worker})_t({period})");
                }
            }
        }

        private void CreateStationAssignmentConstraint() {

            foreach (int period in GetPeriodsList())
            {
                foreach (int station in Instance.GetWorkersList())
                {
                    GRBLinExpr expression = new();
                    foreach (int worker in Instance.GetWorkersList())
                    {
                        expression.AddTerm(1d, YVariables[(station, worker, period)]);
                    }
                    AddConstr(expression, GRB.EQUAL, 1d, $"StationAssignmentOnlyToOneWorkerPerPeriod_s({station})_t({period})");
                }
            }
        }

        public override void Run()
        {
            //Optimize();
        }

        protected override void CompileSolution()
        {

        }

        public override void WriteSolution(Output output)
        {

        }
    }
}
