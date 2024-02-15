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
        public Dictionary<int, List<int>> ImmediateFollowers { get; private set; }
        public Dictionary<int, List<int>> Followers { get; private set; }

        public Input(string fileDirectory, string fileName, int workers, int numberOfTasks, int?[,] matrix, (int, int)[] precedenceGraph)
            : base(fileDirectory, fileName, "")
        {
            Id = InstanceCounter++;
            Workers = workers;
            NumberOfTasks = numberOfTasks;
            Matrix = matrix;
            PrecedenceGraph = precedenceGraph;
            ImmediateFollowers = new Dictionary<int, List<int>>();
            Followers = new Dictionary<int, List<int>>();

            ComputeData();
        }

        private void ComputeData()
        {
            ComputeImmediateFollowers();
            ComputeFollowers();
        }

        private void ComputeImmediateFollowers()
        {
            ImmediateFollowers.Clear();
            GetTasksList().ForEach(t => ImmediateFollowers.Add(t, new List<int>()));

            foreach ((int, int) pair in PrecedenceGraph)
                ImmediateFollowers[pair.Item1].Add(pair.Item2);
        }

        private void ComputeFollowers()
        {
            Followers.Clear();
            foreach (int task in GetTasksList())
                Followers.Add(task, new List<int>(GetFollowers(task)));
        }

        private List<int> GetFollowers(int task)
        {
            List<int> followers = new(ImmediateFollowers[task]);
            followers.ToList().ForEach(x => followers.AddRange(GetFollowers(x)));

            return followers.Distinct().ToList();
        }

        public List<int> GetWorkersList()
        {
            return Enumerable.Range(0, Workers).ToList();
        }

        public List<int> GetTasksList()
        {
            return Enumerable.Range(0, NumberOfTasks).ToList();
        }

        public List<int> GetTasksExecutedByWorker(int worker)
        {
            return GetTasksList().Where(i => GetWorkersWhoCanExecuteTask(i).Contains(worker)).ToList();
        }

        public List<int> GetWorkersWhoCanExecuteTask(int task)
        {
            List<int> workersWhoCanExecuteTask = new();
            foreach (int worker in GetWorkersList())
            {
                if (Matrix[task, worker].HasValue)
                    workersWhoCanExecuteTask.Add(worker);
            }
            return workersWhoCanExecuteTask;
        }

        public List<int> GetWorkersWhoCantExecuteTask(int task)
        {
            List<int> workersWhoCanExecuteTask = new();
            foreach (int worker in GetWorkersList())
            {
                if (!Matrix[task, worker].HasValue)
                    workersWhoCanExecuteTask.Add(worker);
            }
            return workersWhoCanExecuteTask;
        }

        public int? GetTaskTime(int task, int worker)
        {
            return Matrix[task, worker];
        }

        public override void Write()
        {
            throw new NotImplementedException();
        }

        public List<int> GetWorkersExecutionIntersection(List<int> tasksToIntersect, List<int>? tasksToExclude = null)
        {
            if (!tasksToIntersect.Any())
                return new List<int>();

            List<int> result = GetWorkersWhoCanExecuteTask(tasksToIntersect[0]);
            tasksToIntersect.RemoveAt(0);

            foreach (int task in tasksToIntersect)
                result = result.Intersect(GetWorkersWhoCanExecuteTask(task)).ToList();

            if (tasksToExclude != null)
                foreach (int task in tasksToExclude)
                    foreach (int worker in GetWorkersWhoCanExecuteTask(task))
                        result.Remove(worker);

            return result;
        }
    }
}