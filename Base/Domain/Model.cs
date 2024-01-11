using Gurobi;

namespace Base.Domain
{
    public abstract class Model : GRBModel
    {
        private static int InstanceCounter = 0;
        public int Id { get; private set; }
        public int NumberOfPeriods { get; private set; }
        public Input Instance { get; private set; }
        public Solution? Solution { get; private set; }

        public Model(GRBEnv env, int numberOfPeriods, Input instance) : base(env)
        {
            Id = InstanceCounter++;
            NumberOfPeriods = numberOfPeriods;
            Instance = instance;
            Solution = null;
        }

        protected abstract void CreateVariables();

        protected abstract void CreateConstraints();

        public abstract void Run();

        protected abstract void CompileSolution();

        public abstract void WriteSolution(Output output);

        protected void BuildModel()
        {
            CreateVariables();
            CreateConstraints();
        }

        public bool HasSolution()
        {
            return Solution != null;
        }
    }
}