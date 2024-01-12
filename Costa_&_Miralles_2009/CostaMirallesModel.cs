using Base.Domain;
using Gurobi;
using System.Reflection;
using System.Xml.Linq;

namespace Costa_and_Miralles_2009
{
    public class CostaMirallesModel : Model
    {
        public Dictionary<(int, int, int, int), GRBVar> XVariables { get; private set; }
        public Dictionary<(int, int, int), GRBVar> YVariables { get; private set; }
        public Dictionary<(int, int), GRBVar> ZVariables { get; private set; }

        public CostaMirallesModel(GRBEnv env, int numberOfPeriods, Input instance) : base(env, numberOfPeriods, instance)
        {
            XVariables = new();
            YVariables = new();
            ZVariables = new();
            BuildModel();
        }

        protected override void CreateVariables()
        {
            CreateXVariables();
            CreateYVariables();
            CreateZVariables();
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
                            XVariables.Add((station, worker, task, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"x({station}, {worker}, {task}, {period})"));
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
                        YVariables.Add((station, worker, period), AddVar(0d, 1d, 0d, GRB.BINARY, $"y({station}, {worker}, {period})"));
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
                    ZVariables.Add((worker, task), AddVar(0d, 1d, 1d, GRB.BINARY, $"z({worker}, {task})"));
                }
            }            
        }

        protected override void CreateConstraints()
        {

            /*
            // Add Constraints
            GRBConstr constraint = model.AddConstr(x + 2 * y + 3 * z <= 4.0, "c0");

            model.Optimize();

            Console.WriteLine(x.VarName + " " + x.X);
            Console.WriteLine(y.VarName + " " + y.X);
            Console.WriteLine(z.VarName + " " + z.X);
            */
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
