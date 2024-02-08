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
                if (args.Length < 6)
                {
                    throw new Exception("At least four arguments are needed: <InputOrDirectoryPath> <OutputPath> -c <MaximumMeanCycleTime> -t <NumberOfPeriods_1>... (more NumberOfPeriods can be added).");
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

                int positionForCycleTime = args.ToList().IndexOf("-c") + 1;
                if (!int.TryParse(args[positionForCycleTime], out int maximumMeanCycleTime))
                {
                    throw new Exception("Input for MaximumMeanCycleTime is missing or invalid.");
                }

                List<int> periods = new();
                int positionForPeriods = args.ToList().IndexOf("-t") + 1;
                foreach (string numberOfPeriods in args[positionForPeriods..])
                {
                    if (int.TryParse(numberOfPeriods, out int result))
                        periods.Add(result);
                    else
                        break;
                }

                FileAttributes attr = File.GetAttributes(inputFileDirectory);

                List<string> inputFilesPaths = new();
                if (attr.HasFlag(FileAttributes.Directory))
                    foreach (string? file in Directory.GetFiles(inputFileDirectory).OrderBy(x => x).ToList())
                        inputFilesPaths.Add(file);
                else
                    inputFilesPaths.Add(inputFileDirectory);

                // Depending on execution, this list of models is the only thing expected to change
                List<Model.ModelType> models = new() { Model.ModelType.CostaMirallesModel };

                GRBEnv env = new()
                {
                    TimeLimit = 3600,
                    Threads = 1,
                    MIPGap = 1e-3
                };

                foreach (string inputFilePath in inputFilesPaths)
                {
                    try
                    {
                        Input instance = Reader.ReadInputFile(inputFilePath);
                        logger.AddLog($"Running input {instance.FileName}.");
                        foreach (int numberOfPeriods in periods)
                        {
                            logger.AddLog($"Running with {numberOfPeriods} periods.");
                            foreach (Model.ModelType modelType in models)
                            {
                                logger.AddLog($"Running {modelType}.");
                                foreach (Model.ConstraintController constraintController in Enum.GetValues<Model.ConstraintController>())
                                {
                                    // For now, we're ignoring the new constraints
                                    if (constraintController == Model.ConstraintController.FirstExtraConstraint || 
                                        constraintController == Model.ConstraintController.SecondExtraConstraint || 
                                        constraintController == Model.ConstraintController.BothExtraConstraints)
                                    { continue; }

                                    logger.AddLog($"Running with {constraintController} constraint(s).");
                                    try
                                    {
                                        Output output = new(outputFileDirectory, instance.FileName, modelType, constraintController, numberOfPeriods);
                                        if (File.Exists(output.GetFullPath()))
                                        {
                                            throw new Exception($"Output named {output.FileName} already exists. It's execution will be ignored.");
                                        }
                                        env.LogFile = Path.Join(gurobiLogDirectory, $"gurobi_log-{output.FileName}.log");
                                        CostaMirallesModel model = new(env, numberOfPeriods, instance, maximumMeanCycleTime, constraintController);
                                        model.Run();
                                        //model.ComputeIIS();
                                        //model.Write($"{instance.FileName}.ilp");
                                        model.WriteSolution(output);
                                        output.Write();

                                        model.Dispose();
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.AddLog(ex);
                                    }
                                }
                            }
                        }
                        logger.AddLog($"Finished.");
                    }
                    catch (Exception ex)
                    {
                        logger.AddLog(ex);
                    }
                }
                env.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}