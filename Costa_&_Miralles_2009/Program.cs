using Base.DataManagers;
using Base.Domain;
using Gurobi;

namespace Costa_and_Miralles_2009
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    throw new Exception("At least three arguments are needed: <InputOrDirectoryPath> <OutputPath> <NumberOfPeriods_1>... (more NumberOfPeriods can be added)");
                }

                string inputFileDirectory = string.Empty;
                if (args.Length > 0)
                    inputFileDirectory = @args[0];

                string outputFileDirectory = string.Empty;
                if (args.Length > 1)
                    outputFileDirectory = @args[1];

                if (!Directory.Exists(outputFileDirectory))
                    Directory.CreateDirectory(outputFileDirectory);

                string myLogDirectory = Path.Join(outputFileDirectory, @"\ExecutionLogs\");
                if (!Directory.Exists(myLogDirectory))
                    Directory.CreateDirectory(myLogDirectory);

                Logger logger = new(myLogDirectory);

                string gurobiLogDirectory = Path.Join(outputFileDirectory, @"\GurobiLogs\");
                if (!Directory.Exists(gurobiLogDirectory))
                    Directory.CreateDirectory(gurobiLogDirectory);

                List<int> periods = new();
                foreach (string numberOfPeriods in args[2..])
                    periods.Add(int.Parse(numberOfPeriods));

                FileAttributes attr = File.GetAttributes(inputFileDirectory);

                List<string> inputFilesPaths = new();
                if (attr.HasFlag(FileAttributes.Directory))
                    foreach (string? file in Directory.GetFiles(inputFileDirectory).OrderBy(x => x).ToList())
                        inputFilesPaths.Add(file);
                else
                    inputFilesPaths.Add(inputFileDirectory);

                GRBEnv env = new();

                foreach (string inputFilePath in inputFilesPaths)
                {
                    try
                    {
                        Input instance = Reader.ReadInputFile(inputFilePath);
                        logger.AddLog($"Running input {instance.FileName}.");
                        foreach (int numberOfPeriods in periods)
                        {
                            logger.AddLog($"Running with {numberOfPeriods} periods.");
                            try
                            {
                                Output output = new(outputFileDirectory, instance.FileName, nameof(Model.ModelType.CostaMirallesModel), numberOfPeriods);
                                if (File.Exists(output.GetFullPath()))
                                {
                                    throw new Exception($"Output named {output.FileName} already exists. It's execution will be ignored.");
                                }
                                env.LogFile = Path.Join(gurobiLogDirectory, $"gurobi_log-{output.FileName}.log");
                                CostaMirallesModel model = new(env, numberOfPeriods, instance);
                                model.Run();

                                model.WriteSolution(output);
                                output.Write();
                            }
                            catch (Exception ex)
                            {
                                logger.AddLog(ex);
                            }
                        }
                        logger.AddLog($"Finished.");
                    }
                    catch (Exception ex)
                    {
                        logger.AddLog(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}