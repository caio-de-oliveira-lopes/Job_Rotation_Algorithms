using Base.Domain;
using Costa_and_Miralles_2009;
using Gurobi;

namespace Job_Rotation_Algorithms_Comparison
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        public Model? CreateModelByType(Model.ModelType modelType, GRBEnv env, int numberOfPeriods, Input instance, int maximumMeanCycleTime, Model.ConstraintController controller)
        {
            if (modelType == Model.ModelType.CostaMirallesModel)
                return new CostaMirallesModel(env, numberOfPeriods, instance, maximumMeanCycleTime, controller);

            return null;
        }
    }
}