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

        public static string WorkingDirectory => IsLinux ? @"/srv/audio-dictionary" : @"C:\Temp\AudioDictionary";

        public static string WordsFile { get; internal set; }
        public static string OutputResultMp3 { get; internal set; }
        public static string SilenceFile { get; internal set; }
    }
}
