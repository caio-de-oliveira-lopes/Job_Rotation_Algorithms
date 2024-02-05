using Base.Domain;
using System.Text.Json;

namespace Base.DataManagers
{
    public static class Writer
    {
        public static void WriteOutput(Output output, Dictionary<string, object?> data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            //var solutions = instance.Solutions.Values.Select(x => x.Id).ToList();

            File.WriteAllText(output.GetFullPath(), json);
            Console.WriteLine($"\nFinished Writing Output For Instance {output.FileName}.\n");
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
