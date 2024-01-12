using Base.DataManagers;

namespace Base.Domain
{
    public class Logger : BaseFile
    {
        private List<string> Logs { get; set; }
        public int LastWrittenLine { get; private set; }

        public Logger() : base(Directory.GetCurrentDirectory(), CreateFileName(), ".txt")
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
            formattedDateTime = formattedDateTime.Remove(minIndex, 1);

            int secIndex = formattedDateTimeAsArray.Length - 1;

            formattedDateTimeAsArray[hourIndex] = 'h';
            formattedDateTimeAsArray[minIndex + 1] = 'm';
            formattedDateTimeAsArray[secIndex] = 's';

            return $"log_{string.Concat(formattedDateTimeAsArray)}";
        }

        public override void Write()
        {
            Writer.WriteLogs(this);
        }

        public void AddLog(Exception ex)
        {
            AddLog(ex.ToString());
        }

        public void AddLog(string message)
        {
            DateTime currentDateTime = DateTime.Now;
            string formattedDateTime = currentDateTime.ToString("MM/dd/yyyy hh:mm:ss tt");

            string log = $"[{formattedDateTime}]=>{message}";
            Logs.Add(log);
            Console.WriteLine(log);
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
