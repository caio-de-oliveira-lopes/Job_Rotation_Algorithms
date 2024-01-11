using Base.Domain;
using Base.Utils;

namespace Base.DataManagers
{
    public static class Reader
    {
        public static Input ReadInputFile(string inputPath)
        {
            string? line;
            int numberOfWorkers = 0;
            int numberOfTasks = 0;
            int?[,]? matrix = null;
            List<(int, int)> precedenceGraph = new();
            StreamReader sr = new(inputPath);
            string inputName = inputPath.Split(Path.DirectorySeparatorChar).Last();
            string inputDirectory = inputPath.Replace(inputName, "");

            line = sr.ReadLine();

            if (line is null)
                throw new Exception("Not a valid input!");

            // First Line
            numberOfTasks = int.Parse(line);

            // Second Line
            line = sr.ReadLine();
            if (line is null)
                throw new Exception("Not a valid input!");

            string[] splitedLines = line.Split(" ");
            numberOfWorkers = splitedLines.Length;

            matrix = new int?[numberOfTasks, numberOfWorkers];

            int task = 0;
            do
            {
                int worker = 0;
                int?[] values = Array.ConvertAll(splitedLines, i => i.ToNullableInt());
                foreach (int? v in values)
                {
                    matrix[task, worker] = v;
                    worker++;
                }

                line = sr.ReadLine();
                if (line is null)
                    throw new Exception("Not a valid input!");

                splitedLines = line.Split(" ");
                task++;
            } while (task < numberOfTasks);

            int uTask = -1, vTask = -1;
            do
            {
                uTask = int.Parse(splitedLines.First());
                vTask = int.Parse(splitedLines.Last());

                if (uTask != -1 && vTask != -1)
                {
                    precedenceGraph.Add((uTask - 1, vTask - 1));

                    line = sr.ReadLine();
                    if (line is null)
                        throw new Exception("Not a valid input!");

                    splitedLines = line.Split(" ");
                }
            } while (uTask != -1 && vTask != -1);
            sr.Close();

            Console.WriteLine("Input reading finished!");

            if (numberOfWorkers != 0 && numberOfTasks != 0 && matrix is not null)
                return new Input(inputDirectory, inputName, numberOfWorkers, numberOfTasks, matrix, precedenceGraph.ToArray());
            else
                throw new Exception("Not a valid input!");
        }
    }
}
