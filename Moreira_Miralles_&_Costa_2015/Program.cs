using Gurobi;

namespace Moreira_Miralles___Costa_2015
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Out.WriteLine("Usage: tune_cs filename");
                return;
            }

            try
            {
                GRBEnv env = new();

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
                 */

                model.Optimize();

                Console.WriteLine(x.VarName + " " + x.X);
                Console.WriteLine(y.VarName + " " + y.X);
                Console.WriteLine(z.VarName + " " + z.X);
            }
            catch (GRBException e)
            {
                Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
            }
        }
    }
}