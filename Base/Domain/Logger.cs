using Base.DataManagers;

namespace Base.Domain
{
    public class Logger : BaseFile
    {
        private List<string> Logs { get; set; }
        public int LastWrittenLine { get; private set; }

        public Logger(string? logDirectory = null) : base(logDirectory ?? Directory.GetCurrentDirectory(), CreateFileName(), ".log")
        {
            Logs = new List<string>();
            LastWrittenLine = -1;
        }

        private static string CreateFileName()
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("MM/dd/yyyy hh:mm:ss tt");
            formattedDateTime = formattedDateTime.Replace("/", "-");
            formattedDateTime = formattedDateTime.Replace(" ", "-");

            char[] formattedDateTimeAsArray = formattedDateTime.ToArray();
            int hourIndex = formattedDateTime.IndexOf(":");
            formattedDateTime = formattedDateTime.Remove(hourIndex, 1);
            int minIndex = formattedDateTime.IndexOf(":");
            int secIndex = formattedDateTimeAsArray.Length - 1;

            formattedDateTimeAsArray[hourIndex] = 'h';
            formattedDateTimeAsArray[minIndex + 1] = 'm';
            formattedDateTimeAsArray[secIndex] = 's';

            return $"log_{string.Concat(formattedDateTimeAsArray)}";
        }

        public override void Write(params object[] objs)
        {
            Writer.WriteLogs(this);
        }

        public void AddLog(Exception ex, bool writeOnConsole = true)
        {
            AddLog(ex.ToString(), writeOnConsole);
        }

        public void AddLog(string message, bool writeOnConsole = true)
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("MM/dd/yyyy hh:mm:ss tt").Replace(" ", "");

            string log = $"[{formattedDateTime}]=>{message}";
            Logs.Add(log);

            if (writeOnConsole)
                Console.WriteLine(log);

            Write();
        }

        public string[] GetLogs()
        {
            return Logs.ToArray()[(LastWrittenLine + 1)..];
        }

        public void IncrementLastWrittenLine(int numberOfWrittenLines)
        {
            LastWrittenLine += numberOfWrittenLines;
        }
    }
}
