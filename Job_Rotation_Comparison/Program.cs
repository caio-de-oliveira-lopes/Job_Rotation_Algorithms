using Base.DataManagers;
using Base.Domain;
using Base.Utils;
using Borba_and_Ritt_2014;
using Costa_and_Miralles_2009;
using Gurobi;
using Moreira_Miralles_and_Costa_2015;

namespace Job_Rotation_Comparison
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

                bool useRecommendedCycleTime = args.Contains("-rc");
                int? originalMaximumMeanCycleTime = null;
                if (!useRecommendedCycleTime)
                {
                    int positionForCycleTime = args.ToList().IndexOf("-c") + 1;
                    if (!int.TryParse(args[positionForCycleTime], out int maximumMeanCycleTime))
                    {
                        throw new Exception("Input for MaximumMeanCycleTime is missing or invalid.");
                    }
                    originalMaximumMeanCycleTime = maximumMeanCycleTime;
                }

                List<int?> periods = new();
                int positionForPeriods = args.ToList().IndexOf("-t") + 1;
                foreach (string numberOfPeriods in args[positionForPeriods..])
                {
                    if (int.TryParse(numberOfPeriods, out int result))
                        periods.Add(result);
                    else
                        break;
                }

                bool useNumberOfWorkers = args.Contains("-w");
                if (useNumberOfWorkers)
                    periods.Add(null);

                List<double> percentages = new();
                int positionForPercentages = args.ToList().IndexOf("-p") + 1;
                foreach (string percentage in args[positionForPercentages..])
                {
                    if (int.TryParse(percentage, out int result))
                        percentages.Add(1d + (result / 100d));
                    else
                        break;
                }

                if (!percentages.Any())
                    percentages.Add(1d);

                FileAttributes attr = File.GetAttributes(inputFileDirectory);

                List<string> inputFilesPaths = new();
                if (attr.HasFlag(FileAttributes.Directory))
                    foreach (string? file in Directory.GetFiles(inputFileDirectory).OrderBy(x => x).ToList())
                        inputFilesPaths.Add(file);
                else
                    inputFilesPaths.Add(inputFileDirectory);

                // Depending on execution, this list of models is the only thing expected to change
                List<Model.ModelType> models = new() { Model.ModelType.CostaMirallesModel, Model.ModelType.BorbaRittModel };

                GRBEnv env = new()
                {
                    TimeLimit = 3600,
                    Threads = 1,
                    MIPGap = 1e-3
                };
                Model? model = null;

                while (true)
                {
                    foreach (var percentage in percentages)
                    {
                        foreach (int? originalNumberOfPeriods in periods)
                        {
                            foreach (string inputFilePath in inputFilesPaths)
                            {
                                try
                                {
                                    Input instance = Reader.ReadInputFile(inputFilePath);

                                    int cycleTime = useRecommendedCycleTime ?
                                        Util.GetRecommededMaximumMeanCycleTime(instance.FileName) :
                                        originalMaximumMeanCycleTime!.Value;

                                    int maximumMeanCycleTime = (int)Math.Floor(cycleTime * percentage);
                                    int numberOfPeriods = originalNumberOfPeriods ?? instance.Workers;
                                    logger.AddLog($"Running input {instance.FileName} with {numberOfPeriods} periods.");
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
                                                Output output = new(outputFileDirectory, instance.FileName, maximumMeanCycleTime, modelType, constraintController, numberOfPeriods);

                                                string solutionFileName = output.GetFullPath().Replace(".json", ".sol");
                                                string gurobiJsonOutputFileName = solutionFileName.Replace(".sol", "-GUROBI.json");
                                                if (File.Exists(solutionFileName))
                                                {
                                                    /*
                                                    model = CreateModelByType(modelType, env, numberOfPeriods, instance, maximumMeanCycleTime, constraintController) ??
                                                        throw new Exception($"Error when creating model {modelType} for input {instance.FileName}, it will be ignored.");

                                                    model.Update();
                                                    model.Read(solutionFileName);
                                                    env.TimeLimit = 10;
                                                    model.Optimize();
                                                    env.TimeLimit = 3600;

                                                    if (!File.Exists(gurobiJsonOutputFileName))
                                                        Writer.WriteJSON(gurobiJsonOutputFileName, model.GetJSONSolution());*/

                                                    throw new Exception($"Output named {output.FileName.Replace(".json", ".sol")} already exists. It's execution will be ignored.");
                                                }
                                                env.LogFile = Path.Join(gurobiLogDirectory, $"gurobi_log-{output.FileName}.log");

                                                model = CreateModelByType(modelType, env, numberOfPeriods, instance, maximumMeanCycleTime, constraintController) ?? 
                                                    throw new Exception($"Error when creating model {modelType} for input {instance.FileName}, it will be ignored.");

                                                // If infeasible, writes ILP file
                                                //model.WriteILP(output, logger);
                                                //model.Dispose();

                                                // Creates model again to avoid problems with compute IIS
                                                //model = CreateModelByType(modelType, env, numberOfPeriods, instance, maximumMeanCycleTime, constraintController) ??
                                                //    throw new Exception($"Error when creating model {modelType} for input {instance.FileName}, it will be ignored.");

                                                model.WriteLP(output);

                                                model.Run();

                                                model.WriteSolution(output);
                                                output.Write(cycleTime, maximumMeanCycleTime, percentage);
                                                Writer.WriteJSON(gurobiJsonOutputFileName, model.GetJSONSolution());

                                                model.Dispose();
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.AddLog(ex);
                                                model?.Dispose();
                                            }
                                        }
                                    }
                                    logger.AddLog($"Finished.");
                                }
                                catch (Exception ex)
                                {
                                    logger.AddLog(ex);
                                    model?.Dispose();
                                }
                            }
                        }
                    }
                    env.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static Model? CreateModelByType(Model.ModelType modelType, GRBEnv env, int numberOfPeriods, Input instance, int maximumMeanCycleTime, Model.ConstraintController controller)
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