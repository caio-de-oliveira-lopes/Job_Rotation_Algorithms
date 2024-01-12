using Base.Domain;

namespace Base.DataManagers
{
    public static class Writer
    {
        public static void WriteOutput(Output output)
        {

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
