using System.IO;

namespace AudioDictionary
{
    class Environment
    {
        public static bool IsLinux
        {
            get
            {
                int p = (int)System.Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static string WorkingDirectory =>
            IsLinux ? @"/srv/audio-dictionary" : @"C:\Temp\AudioDictionary";

        public static string WordsFile { get; internal set; }
        
        public static void SetOutputResultMp3 (string fileName)
        {
            WorkingPathToResultMp3 = GetWorkingPathToFile(fileName);
        }

        public static void SetSilenceFile(string fileName)
        {
            var binPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            WorkingPathToSilenceFile = Path.Combine(binPath, fileName);
        }

        public static string GetWorkingPathToOgg(string word) =>
            Path.Combine(WorkingDirectory, $"{word}.ogg");

        public static string GetWorkingPathToMp3(string word) =>
            Path.Combine(WorkingDirectory, $"{word}.mp3");

        public static string WorkingPathToResultMp3 { get; private set; }

        public static string GetWorkingPathToFile(string fileName) =>
            Path.Combine(WorkingDirectory, fileName);

        public static string WorkingPathToSilenceFile { get; private set; }
    }
}
