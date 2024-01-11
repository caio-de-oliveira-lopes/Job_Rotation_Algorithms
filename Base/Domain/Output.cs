using Base.DataManagers;

namespace Base.Domain
{
    public class Output : BaseFile
    {
        private static int InstanceCounter = 0;
        public int Id { get; private set; }

        public Output(string fileDirectory, string fileName)
            : base(fileDirectory, $"output{fileName}", ".json")
        {
            Id = InstanceCounter++;
        }

        public override void Write()
        {
            Writer.WriteOutput(this);
        }
    }
}