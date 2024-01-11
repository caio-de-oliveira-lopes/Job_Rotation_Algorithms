using Base.Domain;
using System.Text;

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
                StreamWriter sw = new(logger.GetFullPath(), true, Encoding.UTF8);
                var logs = logger.GetLogs();
                foreach (string line in logs)
                    sw.WriteLine(line);

                sw.Close();
                logger.IncrementLastWrittenLine(logs.Count());
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Finished Writing Logs.");
            }
        }
    }
}
