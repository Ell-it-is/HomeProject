using System.IO;
using System.Text;

namespace Odberatele
{
    public class FileWriter
    {
        private FileStream fs;
        private StreamWriter sw;
        private string _path = @"ValidationErrors.txt";

        public FileWriter()
        {
            fs = new FileStream(_path, FileMode.Create);
        }
        
        public void Logger()
        {
            fs = new FileStream(_path, FileMode.Append, FileAccess.Write);
            sw = new StreamWriter(fs, Encoding.UTF8);
        }

        public void Log(string message)
        {
            sw.WriteLine(message);
            sw.Flush();
            fs.Flush();
        }

        public void Dispose()
        {
            sw?.Dispose();
            fs?.Dispose();
        }
    }
}