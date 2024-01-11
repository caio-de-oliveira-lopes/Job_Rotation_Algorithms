using Base.DataManagers;

namespace Base.Domain
{
    public class Logger : BaseFile
    {
        private List<string> Logs { get; set; }

        public Logger(string fileDirectory, string fileName, string extension) : base(fileDirectory, fileName, extension)
        {
            Logs = new List<string>();
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
    }
}
