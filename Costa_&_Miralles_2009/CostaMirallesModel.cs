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
            foreach (int station in Enumerable.Range(0, Instance.Workers))
            {
                foreach (int worker in Enumerable.Range(0, Instance.Workers))
                {
                    foreach (int task in Enumerable.Range(0, Instance.NumberOfTasks))
                    {
                        foreach (int period in Enumerable.Range(0, NumberOfPeriods))
                        {
                            XVariables.Add((station, worker, task, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"X(s({station})_w({worker})_i({task})_t({period}))"));
                        }
                    }
                }
            }
        }

        private void CreateYVariables()
        {
            foreach (int station in Enumerable.Range(0, Instance.Workers))
            {
                foreach (int worker in Enumerable.Range(0, Instance.Workers))
                {
                    foreach (int period in Enumerable.Range(0, NumberOfPeriods))
                    {
                        YVariables.Add((station, worker, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"Y(s({station})_w({worker})_t({period}))"));
                    }
                }
            }
        }

        private void CreateZVariables()
        {
            foreach (int worker in Enumerable.Range(0, Instance.Workers))
            {
                foreach (int task in Enumerable.Range(0, Instance.NumberOfTasks))
                {
                    ZVariables.Add((worker, task), AddVar(0d, 1d, 1d, GRB.BINARY, $"Z(w({worker})_t({task}))"));
                }
            }            
        }

        protected override void CreateConstraints()
        {
            CreateWorkerAssignmentConstraint(); // 1.4
            CreateStationAssignmentConstraint(); // 1.5
        }

        protected override void AddExtraConstraints()
        {
            if (Controller != ConstraintController.None)
            {
                if (Controller != ConstraintController.SecondConstraint) { }
                    //CreateFirstExtraConstraint(); // 1.25
                if (Controller != ConstraintController.FirstConstraint) { }
                    //CreateSecondExtraConstraint(); // 1.26
            }
        }

        private void CreateWorkerAssignmentConstraint()
        {
            foreach (int period in Enumerable.Range(0, NumberOfPeriods))
            {
                foreach (int worker in Enumerable.Range(0, Instance.Workers))
                {
                    GRBLinExpr expression = new();
                    foreach (int station in Enumerable.Range(0, Instance.Workers))
                    {
                        expression.AddTerm(1d, YVariables[(station, worker, period)]);
                    }
                    AddConstr(expression, GRB.EQUAL, 1d, $"WorkerAssignedOnlyToOneStationPerPeriod_w({worker})_t({period})");
                }
            }
        }

        private void CreateStationAssignmentConstraint() {

            foreach (int period in Enumerable.Range(0, NumberOfPeriods))
            {
                foreach (int station in Enumerable.Range(0, Instance.Workers))
                {
                    GRBLinExpr expression = new();
                    foreach (int worker in Enumerable.Range(0, Instance.Workers))
                    {
                        expression.AddTerm(1d, YVariables[(station, worker, period)]);
                    }
                    AddConstr(expression, GRB.EQUAL, 1d, $"StationAssignedOnlyToOneWorkerPerPeriod_s({station})_t({period})");
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
