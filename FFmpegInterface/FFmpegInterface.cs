using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using FFmpegLibrary;

namespace FFmpegInterface
{
    class FFmpegInterface
    {
        static FFmpegLibrary.FFmpegLibrary libraryDll; //экземпляр класса библиотеки

        public static void Main(string[] args)
        {
            Menu();
        }

        public static void Menu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine();
                Console.WriteLine("\t\t\t<Меню>");
                Console.WriteLine();
                Console.WriteLine("\t1. Получить информацию о медиа-файле");
                Console.WriteLine("\t2. Извлечь аудиодорожку");
                Console.WriteLine("\t3. Нарезать аудиодорожку");
                Console.WriteLine("\t4. Нормализовать звук");
                Console.WriteLine("\t5. Выйти из программы");
                Console.WriteLine();
                Console.Write("\t\tВыберите пункт меню -> "); int tmp = Int32.Parse(Console.ReadLine());

                switch (tmp)
                {
                    case 1:
                        CaseOne(); //получить информацию о медиа-файле
                        break;

                    case 2:
                        CaseTwo(); //извлечь аудиодорожку
                        break;

                    case 3:
                        CaseThree(); //нарезать аудиодорожку
                        break;

                    case 4:
                        CaseFour(); //нормализовать звук
                        break;

                    case 5:
                        Environment.Exit(0);
                        break;
                }
            }
        }

        public static void CaseOne()
        {
            List<string> infoFile = new List<string>(); //хранит информацию о файле

            infoFile = libraryDll.FileInformation(@"D:\Internatura\practice\1TVCH\WCIYD_201.mxf");

            Console.WriteLine();
            Console.Write("\t\tГотово! ");
            Console.ReadLine();
        }

        public static void CaseTwo()
        {
            string outPath = System.String.Empty; //хранит путь к извлеченной аудиодорожке

            bool hasProcess = true; //хранит значение, указывающее, нужно ли выполнять процесс

            if (File.Exists(@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\audio.wav"))
            {
                Console.WriteLine();
                Console.Write($"   Файл с именем {Path.GetFileName(@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\audio.wav")} уже существует. " +
                    "Перезаписать? Y/y-да, N/n-нет -> ");
                string check = Console.ReadLine();
                if (check == "N" || check == "n")
                {
                    hasProcess = false;
                }
            }

            if (hasProcess)
            {
                outPath = libraryDll.ExtractAudio(@"D:\Internatura\practice\1TVCH\WCIYD_201.mxf", 44100, 2, 192, 1, @"D:\Internatura\practice\1TVCH\audio.wav");
                Console.ReadLine();
            }
            else
                outPath = "";
        }

        public static void CaseThree()
        {
            string cutPath = ""; //хранит путь к нарезанной дорожке

            Console.WriteLine();
            Console.WriteLine("\tЗадайте начало и конец нарезки в формате [hh:mm:ss.ms]:");
            Console.WriteLine();
            Console.Write("\tstart -> "); string start = Console.ReadLine();
            Console.Write("\tend   -> "); string end = Console.ReadLine();

            bool hasProcess = true; //хранит значение, указывающее, нужно ли выполнять процесс

            if (File.Exists($@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\CuttingTrack\audio_{start.Replace(":", ".")}_{end.Replace(":", ".")}.wav"))
            {
                Console.WriteLine();
                string tmp = Path.GetFileName($@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\audio_{start.Replace(":", ".")}_{end.Replace(":", ".")}.wav");
                Console.Write($"   Файл с именем {tmp} уже существует. " +
                    "Перезаписать? Y/y-да, N/n-нет -> ");
                string check = Console.ReadLine();
                if (check == "N" || check == "n")
                {
                    hasProcess = false;
                }
            }

            if (hasProcess)
            {
                cutPath = libraryDll.CuttingAudioTrack(@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\audio.wav", start, end, @"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\CuttingTrack");
                Console.ReadLine();
            }
            else
                cutPath = "";
        }

        public static void CaseFour()
        {
            string path = ""; //хранит путь к нормализованной дорожке
           
            bool hasProcess = true; //хранит значение, указывающее, нужно ли выполнять процесс

            if (File.Exists($@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\normalAudio.wav"))
            {
                Console.WriteLine();
                string tmp = Path.GetFileName($@"D:\Bitbucket.org\Cosmonaut13\ffmpeg-wrapper\WorkSpace\audio\normalAudio.wav");
                Console.Write($"   Файл с именем {tmp} уже существует. " +
                    "\n   Перезаписать? Y/y-да, N/n-нет -> ");
                string check = Console.ReadLine();
                if (check == "N" || check == "n")
                {
                    hasProcess = false;
                }
            }

            if (hasProcess)
            {
                Console.WriteLine();
                Console.Write("\tЗадайте уровень нормализации в формате [+/-dB] -> "); string normalLevel = Console.ReadLine();
                Console.WriteLine();
                Console.Write("\t\tПодождите...");
                path = libraryDll.SoundNormalization(@"D:\Internatura\practice\1TVCH\audio.wav", normalLevel, @"D:\Internatura\practice\1TVCH\normalAudio.wav");
                Console.WriteLine();
                Console.Write("\t\tГотово! ");
                Console.ReadLine();
            }
            else
                path = "";
        }

        //вызывается при срабатывании события OnFFmpegProgressChanged
        static void libraryDll_FFmpegProgressChanged(object sender, FFmpegProgressChangedEventArgs e)
        {
            Console.WriteLine();
            double percent = Math.Round(e._currentDuration.TotalSeconds / e._duration.TotalSeconds * 100);
            Console.WriteLine($"    Обработано {percent}% медиа-файла!");
        }

        static FFmpegInterface()
        {
            libraryDll = new FFmpegLibrary.FFmpegLibrary(new TextFileLogger(Directory.GetCurrentDirectory()));
            libraryDll.FFmpegProgressChanged += libraryDll_FFmpegProgressChanged;
        }
    }
}
