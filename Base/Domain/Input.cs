namespace Base.Domain
{
    public class Input : BaseFile
    {
        private static int InstanceCounter = 0;
        public int Id { get; private set; }
        public int Workers { get; private set; }
        public int NumberOfTasks { get; private set; }
        public int?[,] Matrix { get; private set; }
        public (int, int)[] PrecedenceGraph { get; private set; }

        public Input(string fileDirectory, string fileName, int workers, int numberOfTasks, int?[,] matrix, (int, int)[] precedenceGraph)
            : base(fileDirectory, fileName, "")
        {
            Id = InstanceCounter++;
            Workers = workers;
            NumberOfTasks = numberOfTasks;
            Matrix = matrix;
            PrecedenceGraph = precedenceGraph;
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }
    }
}