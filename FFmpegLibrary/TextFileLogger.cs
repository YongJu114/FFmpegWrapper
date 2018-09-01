using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FFmpegLibrary
{
    public class TextFileLogger : ILog
    {
        public void Log(string Message)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{_directory}\\logFFMPEG.txt", true))
            {
                file.WriteLine($"{DateTime.Now}\t\t{Message}");
                file.Close();
            }
        }

        private string _directory;

        public TextFileLogger(string directory)
        {
            this._directory = directory;
        }
    }
}
