using Base.DataManagers;

namespace Base.Domain
{
    public class Logger : BaseFile
    {
        private List<string> Logs { get; set; }
        public int LastWrittenLine { get; private set; }

        public Logger(string fileDirectory, string fileName, string extension) : base(fileDirectory, fileName, extension)
        {
            Logs = new List<string>();
            LastWrittenLine = -1;
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
