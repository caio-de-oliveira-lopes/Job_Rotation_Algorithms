using Base.Domain;
using Gurobi;

namespace Costa___Miralles_2009
{
    public class CostaMirallesModel : Model
    {
        public CostaMirallesModel(GRBEnv env, int numberOfPeriods, Input instance) : base(env, numberOfPeriods, instance)
        {
            BuildModel();
        }

        protected override void CreateVariables()
        {
            /*
            GRBModel model = new(env);
            // Add Variables
            GRBVar x = model.AddVar(0d, 1d, 0d, GRB.BINARY, "x");
            GRBVar y = model.AddVar(0d, 1d, 0d, GRB.BINARY, "y");
            GRBVar z = model.AddVar(0d, 1d, 0d, GRB.BINARY, "z");

            // Add Constraints
            GRBConstr constraint = model.AddConstr(x + 2 * y + 3 * z <= 4.0, "c0");

            /*
            * Once the model has been built, the typical next step is to optimize it (using GRBoptimize in C, model.optimize in C++, 
            * Java, and Python, or model.Optimize in C#). You can then query the X attribute on the variables to retrieve the solution 
            * (and the VarName attribute to retrieve the variable name for each variable). In C, the X attribute is retrieved as follows


            model.Optimize();

            Console.WriteLine(x.VarName + " " + x.X);
            Console.WriteLine(y.VarName + " " + y.X);
            Console.WriteLine(z.VarName + " " + z.X);
            */
        }

        protected override void CreateConstraints()
        {

        }

        public override void Run()
        {

        }

        protected override void CompileSolution()
        {

        }

        public override void WriteSolution(Output output)
        {

        }
    }
}
