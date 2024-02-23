namespace Base.Domain
{
    public abstract class BaseFile
    {
        public string FileDirectory { get; private set; }
        public string FileName { get; private set; }
        public string Extension { get; private set; }

        public BaseFile(string fileDirectory, string fileName, string extension)
        {
            FileDirectory = fileDirectory;
            FileName = fileName;
            Extension = extension;
        }

        public abstract void Write(params object[] objs);

        public string GetFullPath()
        {
            return Path.Combine(FileDirectory, $"{FileName}{Extension}");
        }
    }
}