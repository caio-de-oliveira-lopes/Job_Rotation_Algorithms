using Base.Domain;
using Borba_and_Ritt_2014;
using Costa_and_Miralles_2009;
using Gurobi;
using Moreira_Miralles_and_Costa_2015;

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
            if (modelType == Model.ModelType.MoreiraMirallesCostaModel)
                return new MoreiraMirallesCostaModel(env, numberOfPeriods, instance, maximumMeanCycleTime, controller);
            if (modelType == Model.ModelType.BorbaRittModel)
                return new BorbaRittModel(env, numberOfPeriods, instance, maximumMeanCycleTime, controller);

            return null;
        }
    }
}