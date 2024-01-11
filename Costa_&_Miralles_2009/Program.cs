using Base.DataManagers;
using Base.Domain;
using Gurobi;

namespace Costa___Miralles_2009
{
    public class Program
    {
        static void Main(string[] args)
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("MM/dd/yyyy hh:mm:ss tt");
            formattedDateTime = formattedDateTime.Replace("/", "-");
            formattedDateTime = formattedDateTime.Replace(" ", "_");

            Logger logger = new(Directory.GetCurrentDirectory(), $"log_{formattedDateTime}", ".txt");
            //Logger logger = new(Directory.GetCurrentDirectory(), $"log", ".txt");

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
                                Output output = new(outputFileDirectory, instance.FileName);

                                if (File.Exists(output.GetFullPath()))
                                {
                                    throw new Exception($"Output named {output.FileName} already exists. It's execution will be ignored.");
                                }

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
                        logger.Write();
                    }
                    catch (Exception ex)
                    {
                        logger.AddLog(ex);
                    }
                }
                logger.Write();
            }
            catch (GRBException ex)
            {
                logger.AddLog($"Error code: {ex.ErrorCode}.{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.AddLog(ex);
            }

            logger.Write();
        }
    }
}