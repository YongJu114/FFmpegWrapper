using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace FFmpegLibrary
{
    public delegate void FFmpegProgressChangedEventHandler(object sender, FFmpegProgressChangedEventArgs e);

    public class FFmpegLibrary
    {
        ILog _logger;

        public TimeSpan duration;
        public int currentSize;
        public TimeSpan currentDuration;
        public double currentBitrate;
        public double currentSpeed;

        public FFmpegProgressChangedEventArgs progressChanged; //класс, который хранит размер, длительность, bitrate, speed на текущем этапе обработки медиа-файла и общую длительность

        //генерация процесса FFMPEG
        private Process CreateProcessWithArguments(string nameProcess, string arguments)
        {
            Process process = new Process();

            process.StartInfo.FileName = nameProcess;
            process.StartInfo.Arguments = arguments;

            process.StartInfo.UseShellExecute = false; //не используется оболочка операционной системы для запуска процесса
                                                       //процесс создаётся непосредственно из исполняемого файла

            process.StartInfo.RedirectStandardError = true; //выходные данные процесса записывается в поток Process.StandardError

            process.StartInfo.CreateNoWindow = true; //процесс запускается без создания для него нового окна

            return process;
        }

        //информация о файле  
        public List<string> FileInformation(string inputPath)
        {
            List<string> fileInfo = new List<string>(); //хранит информацию о файле

            Process process = CreateProcessWithArguments("ffmpeg.exe", $"-i {inputPath} -hide_banner");
            process.Start();

            _logger.Log($"ffmpeg -i {inputPath} -hide_banner");

            while (!process.StandardError.EndOfStream)
            {
                string buffer = process.StandardError.ReadLine(); //буфер, в который помещаются данные FFMPEG
                _logger.Log(buffer); //записываем буфер в файл лога
                fileInfo.Add(buffer); //добавляем буфер в лист
            }
            _logger.Log(""); //табуляция
            _logger.Log(""); //табуляция
            return fileInfo;
        }

        //извлечение аудиодорожки 
        public string ExtractAudio(string inputPath, int samplingFrequency, int codecTracks, int bitrate, int stream, string outputPath)
        {
            Process process = CreateProcessWithArguments("ffmpeg.exe", $"-i {inputPath} -vn -ar {samplingFrequency} -ac {codecTracks} -ab {bitrate} -map 0:{stream} -f wav -y {outputPath} -hide_banner");
            process.Start();

            _logger.Log($"ffmpeg -i {inputPath} -vn -ar {samplingFrequency} -ac {codecTracks} -ab {bitrate} -map 0:{stream} -f wav -y {outputPath} -hide_banner");

            while (!process.StandardError.EndOfStream)
            {
                string buffer = process.StandardError.ReadLine(); 
                _logger.Log(buffer); 

                PatternString(buffer);
            }
            _logger.Log(""); //табуляция
            _logger.Log(""); //табуляция
            return outputPath;
        }

        //нарезка аудиодорожки
        public string CuttingAudioTrack(string inputPath, string _start, string _end, string outputPath)
        {
            string path = ""; //хранит путь к нарезонной аудиодорожке
            string audio = Path.GetFileNameWithoutExtension(inputPath); //хранит имя аудиодорожки, которую необходимо нарезать
                                                                        //используется для задания имени нарезок
            TimeSpan dif = GetDifTimeSpanFromDuration(_start, _end); //хранит длительность нарезки

            Process process = CreateProcessWithArguments("ffmpeg.exe", $"-i {inputPath} -ss {_start} -t {dif.ToString()} -acodec pcm_s16le -ar 44100 -y {outputPath}\\{audio}_{_start.Replace(":", ".")}_{_end.Replace(":", ".")}.wav -hide_banner");
            process.Start();

            _logger.Log($"ffmpeg -i {inputPath} -ss {_start} -t {dif.ToString()} -acodec pcm_s16le -ar 44100 -y {outputPath}\\{audio}_{_start.Replace(":", ".")}_{_end.Replace(":", ".")}.wav -hide_banner");
            path = $"{ outputPath}\\{ audio}_{ _start.Replace(":", ".")}_{ _end.Replace(":", ".")}.wav";

            while (!process.StandardError.EndOfStream)
            {
                string buffer = process.StandardError.ReadLine();
                _logger.Log(buffer); 

                PatternString(buffer, dif);
            }
            _logger.Log(""); //табуляция
            _logger.Log(""); //табуляция
            return path;
        }

        //нрмализация звука
        public string SoundNormalization(string inputPath, string level, string outputPath)
        {
            Process process = CreateProcessWithArguments("sox.exe", $@"-V4 {inputPath} {outputPath} gain -n {level} spectrogram -o {Path.GetDirectoryName(outputPath)}\Spectrogram\{Path.GetFileNameWithoutExtension(outputPath)}_spectrogram.png");
            process.Start();

            _logger.Log($@"sox -V4 {inputPath} {outputPath} gain -n {level} spectrogram -o {Path.GetDirectoryName(outputPath)}\Spectrogram\{Path.GetFileNameWithoutExtension(outputPath)}_spectrogram.png");
            while (!process.StandardError.EndOfStream)
            {
                string buffer = process.StandardError.ReadLine();
                _logger.Log(buffer);
            }
            _logger.Log(""); //табуляция
            _logger.Log(""); //табуляция
            return outputPath;
        }

        //разница времени между start и end
        private TimeSpan GetDifTimeSpanFromDuration(string start, string end)
        {
            string pattern = @"[:,:,:,.]"; //Шаблон для парсинга

            string[] split = Regex.Split(start, pattern, RegexOptions.IgnoreCase);
            int hours = Int32.Parse(split[0]); ;
            int minutes = Int32.Parse(split[1]);
            int seconds = Int32.Parse(split[2]);
            int milliseconds = Int32.Parse(split[3]) * 10;
            TimeSpan ts1 = new TimeSpan(0, hours, minutes, seconds, milliseconds);

            split = Regex.Split(end, pattern, RegexOptions.IgnoreCase);
            hours = Int32.Parse(split[0]); ;
            minutes = Int32.Parse(split[1]);
            seconds = Int32.Parse(split[2]);
            milliseconds = Int32.Parse(split[3]) * 10;
            TimeSpan ts2 = new TimeSpan(0, hours, minutes, seconds, milliseconds);

            return ts2 - ts1;
        }

        //сплитим buffer
        private void PatternString(string _buffer)
        {
            string patternData = @"\w+:\s(\d{2}:\d{2}:\d{2}\.\d{2}),\s*[\w+]*:*\s*\d*\.*[\d+]*,*\s\w+:\s(\d+\.*\d*)\s(\w+\W\w)";
            Regex rgxData = new Regex(patternData);
            string[] splitData;

            string patternProgressChanged = @"(^\w+)\W\s*(\d+)(\w+)\s(\w+)\W(\d{2}:\d{2}:\d{2}\.\d{2})\s(\w+)\W\s*(\d+\.*\d*)(\w+\W\w)\s(\w+)\W\s*(\d+\.*\d*)(\w)";
            Regex rgxProgressChanged = new Regex(patternProgressChanged);
            string[] splitProgressChanged;

            if (rgxData.IsMatch(_buffer))
            {
                splitData = Regex.Split(_buffer, patternData, RegexOptions.IgnoreCase);
                GetDataFromCurrentString(splitData[1]); 
            }

            if (rgxProgressChanged.IsMatch(_buffer))
            {
                splitProgressChanged = Regex.Split(_buffer, patternProgressChanged, RegexOptions.IgnoreCase);
                GetProgressChangedFromCurrentString(splitProgressChanged[2], splitProgressChanged[5], splitProgressChanged[7], splitProgressChanged[10]);

                OnFFmpegProgressChanged(new FFmpegProgressChangedEventArgs(duration, currentSize, currentDuration, currentBitrate, currentSpeed));
            }
        }

        //перегруженный метод, который сплитит buffer
        private void PatternString(string _buffer, TimeSpan _dif)
        {
            string patternProgressChanged = @"(^\w+)\W\s*(\d+)(\w+)\s(\w+)\W(\d{2}:\d{2}:\d{2}\.\d{2})\s(\w+)\W\s*(\d+\.*\d*)(\w+\W\w)\s(\w+)\W\s*(\d+\.*\d*)(\w)";
            Regex rgxProgressChanged = new Regex(patternProgressChanged);
            string[] splitProgressChanged;

            if (rgxProgressChanged.IsMatch(_buffer))
            {
                splitProgressChanged = Regex.Split(_buffer, patternProgressChanged, RegexOptions.IgnoreCase);
                GetProgressChangedFromCurrentString(splitProgressChanged[2], splitProgressChanged[5], splitProgressChanged[7], splitProgressChanged[10]);

                OnFFmpegProgressChanged(new FFmpegProgressChangedEventArgs(_dif, currentSize, currentDuration, currentBitrate, currentSpeed));
            }
        }

        //парсим строку и задаем значение для duration
        private void GetDataFromCurrentString(string _duration)
        {
            string pattern = @"[:,:,:,.]"; 
            string[] split = Regex.Split(_duration, pattern, RegexOptions.IgnoreCase); 

            int hours = Int32.Parse(split[0]); ;
            int minutes = Int32.Parse(split[1]);
            int seconds = Int32.Parse(split[2]);
            int milliseconds = Int32.Parse(split[3]) * 10;

            duration = new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        //парсим строку и задаём значения для currentSize, currentDuration, currentBitrate, currentSpeed
        private void GetProgressChangedFromCurrentString(string _currentSize, string _currentDuration, string _currentBitrate, string _currentSpeed)
        {
            string pattern = @"[:,:,:,.]"; 
            string[] split = Regex.Split(_currentDuration, pattern, RegexOptions.IgnoreCase); 

            int hours = Int32.Parse(split[0]); ;
            int minutes = Int32.Parse(split[1]);
            int seconds = Int32.Parse(split[2]);
            int milliseconds = Int32.Parse(split[3]) * 10;

            currentSize = Int32.Parse(_currentSize);
            currentDuration = new TimeSpan(0, hours, minutes, seconds, milliseconds);
            currentBitrate = Double.Parse(_currentBitrate.Replace(".", ","));
            currentSpeed = Double.Parse(_currentSpeed.Replace(".", ","));
        }

        public event FFmpegProgressChangedEventHandler FFmpegProgressChanged;

        protected virtual void OnFFmpegProgressChanged(FFmpegProgressChangedEventArgs e)
        {
            var _event = FFmpegProgressChanged;
            if (_event != null)
            {
                FFmpegProgressChanged(this, e);
            }
        }

        //конструктор класса FFmpegLibrary
        public FFmpegLibrary(ILog logger)
        {
            _logger = logger;
        }
    }
}
