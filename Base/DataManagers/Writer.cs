using Base.Domain;
using System.Text.Json;

namespace Base.DataManagers
{
    public static class Writer
    {
        public static void WriteOutput(Output output, Dictionary<string, object?> data)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            //var solutions = instance.Solutions.Values.Select(x => x.Id).ToList();

            File.WriteAllText(output.GetFullPath(), json);
            Console.WriteLine($"\nFinished Writing Output For Instance {output.FileName}.\n");
        }

        public static void WriteJSON(string filePath, string jsonString)
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            string json = JsonSerializer.Serialize(jsonString, options);

            File.WriteAllText(filePath, json);

            Console.WriteLine($"\nFinished Writing Gurobi JSON Output.\n");
        }

        public static void WriteLogs(Logger logger)
        {
            try
            {
                string[] logs = logger.GetLogs();
                using FileStream fileStream = File.Open(logger.GetFullPath(), FileMode.Append);
                using (StreamWriter sw = new(fileStream))
                {
                    foreach (string line in logs)
                        sw.WriteLine(line);
                }
                logger.IncrementLastWrittenLine(logs.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
