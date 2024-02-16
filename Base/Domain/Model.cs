using Gurobi;

namespace Base.Domain
{
    public abstract class Model : GRBModel
    {
        private static int InstanceCounter = 0;
        public int Id { get; private set; }
        public int NumberOfPeriods { get; private set; }
        public Input Instance { get; private set; }
        public Solution? Solution { get; protected set; }

        public Model(GRBEnv env, int numberOfPeriods, Input instance) : base(env)
        {
            Id = InstanceCounter++;
            NumberOfPeriods = numberOfPeriods;
            Instance = instance;
            Solution = null;
        }

        public enum ModelType
        {
            CostaMirallesModel,
            BorbaRittModel,
            MoreiraMirallesCostaModel
        }

        public enum ConstraintController
        {
            NoExtraConstraint,
            FirstExtraConstraint,
            SecondExtraConstraint,
            BothExtraConstraints
        }

        public List<int> GetPeriodsList()
        {
            return Enumerable.Range(0, NumberOfPeriods).ToList();
        }

        protected abstract void CreateVariables();

        protected abstract void CreateConstraints();

        protected abstract void DefineSense();

        public abstract void Run();

        protected abstract void CompileSolution();

        public void WriteSolution(Output output)
        {
            WriteSOL(output);
            CompileSolution();
            if (HasSolution())
                output.SetSolution(Solution!);
        }

        protected void BuildModel()
        {
            CreateVariables();
            CreateConstraints();
            AddExtraConstraints();
            DefineSense();
        }

        protected abstract void AddExtraConstraints();

        public bool HasSolution()
        {
            return Solution != null;
        }

        public void WriteILP(Output output, Logger? logger = null)
        {
            try
            {
                ComputeIIS();
                Write($"{output.GetFullPath().Replace(".json", ".ilp")}");
            }
            catch (Exception ex)
            {
                logger?.AddLog(ex, false);
            }
        }

        public void WriteLP(Output output)
        {
            Write($"{output.GetFullPath().Replace(".json", ".lp")}");
        }

        public void WriteSOL(Output output)
        {
            Write($"{output.GetFullPath().Replace(".json", ".sol")}");
        }
    }
}