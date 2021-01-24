using NAudio.MediaFoundation;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace AudioDictionary
{
    class Program
    {
        private const string UlrEnBase = "https://www.oxfordlearnersdictionaries.com/us/media/english/us_pron_ogg";
        private const string UrlEnPostfix = "__us_1.ogg";
        private const string UrlWikiBaseOgg = "https://upload.wikimedia.org/wikipedia/commons";
        private const string UrlRuWikiBaseHtml = "https://ru.wiktionary.org/wiki";
        private const string UrlEnWikiBaseHtml = "https://en.wiktionary.org/wiki";
        private const string WorkingDirectory = @"/srv/audio-dictionary";
        private const string Silence05sec = "silence-0.5s.mp3";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (!IsLinux)
                MediaFoundationInterop.MFStartup(0);

            var wordsFile = args.Length > 0 ? args[0] : @"/tmp/words-list.txt";
            var outputResultMp3 = args.Length > 1 ? args[1] : "!result.mp3";

            Console.WriteLine($"Reading words list from {wordsFile}");

            // 1. Get list of files to download
            var wordsList = ReadFilesListToDownload(wordsFile);

            // 2. Download files
            DownloadAudio(wordsList);

            // 3. Convert .ogg to .mp3 files
            Console.WriteLine();
            Console.WriteLine("Converting donwloaded OGG files to MP3");
            ConvertDownloadedAudio(wordsList);

            // 4. Normalize .mp3 files
            NormalizeAudio(wordsList);

            // 5. Merge all files into one result mp3
            Console.WriteLine();
            Console.WriteLine("Merging all files into one result MP3");
            MergeFiles(wordsList, Path.Combine(WorkingDirectory, outputResultMp3));

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Done. {wordsList.Count(w => w.HasAudio)} words has been merged.");
        }

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static void NormalizeAudio(List<Word> wordsList)
        {
            // Trim empy sound
            // Normalize Volume
            // Speed/Tempo?
        }

        private static void MergeFiles(List<Word> wordsList, string outputFile)
        {
            if (IsLinux)
                MergeFilesLinux(wordsList, outputFile);
            else
                MergeFilesWindows(wordsList, outputFile);
        }

        private static void MergeFilesLinux(List<Word> wordsList, string outputFile)
        {
            var lines = new List<string>();

            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                lines.Add($"file '{GetFullPath(word.Russian)}.mp3'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(word.English)}.mp3'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(word.English)}.mp3'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(word.English)}.mp3'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(word.English)}.mp3'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(word.English)}.mp3'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
                lines.Add($"file '{GetFullPath(Silence05sec)}'");
            }

            var textFile = Path.Combine(WorkingDirectory, "mylist.txt");

            File.WriteAllLines(textFile, lines);

            Thread.Sleep(1000);

            CopySilenceToWorkingDir();
            ExecuteFFMPEG($" -y -f concat -safe 0 -i {textFile} -c copy {outputFile}");
        }

        private static void CopySilenceToWorkingDir()
        {
            var binPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var fullSilencePathFrom = Path.Combine(binPath, Silence05sec);
            var fullSilencePathTo = GetFullPath(Silence05sec);
            
            File.Copy(fullSilencePathFrom, fullSilencePathTo, true);
        }

        private static string GetFullPath(string fileName)
        {
            return Path.Combine(WorkingDirectory, fileName);
        }

        private static void MergeFilesWindows(List<Word> wordsList, string outputFile)
        {
            var outputStream = new FileStream(Path.Combine(WorkingDirectory, outputFile), FileMode.Create);

            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                var fileName = Path.Combine(WorkingDirectory, $"{word.Russian}.mp3");
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(Silence05sec, outputStream);

                fileName = Path.Combine(WorkingDirectory, $"{word.English}.mp3");
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);

                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
            }
        }

        private static void MergeFileWindows(string fileName, FileStream outputStream)
        {
            var reader = new Mp3FileReader(fileName);

            if ((outputStream.Position == 0) && (reader.Id3v2Tag != null))
            {
                outputStream.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
            }
            Mp3Frame frame;
            while ((frame = reader.ReadNextFrame()) != null)
            {
                outputStream.Write(frame.RawData, 0, frame.RawData.Length);
            }
        }

        private static void ConvertDownloadedAudio(List<Word> wordsList)
        {
            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                word.HasAudio = ConvertOggToMp3(Path.Combine(WorkingDirectory, $"{word.English}.ogg"));
                word.HasAudio &= ConvertOggToMp3(Path.Combine(WorkingDirectory, $"{word.Russian}.ogg"));
            }
        }

        private static bool ConvertOggToMp3(string fileName)
        {
            try
            {
                EncodeToMp3(fileName);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"(!) No Suitable Encoder for {Path.GetFileNameWithoutExtension(fileName)}");
                Console.WriteLine("XXXXXXXXXXXX");
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        private static void EncodeToMp3(string fileName)
        {
            if (IsLinux)
                EncodeToMp3Linux(fileName);
            else
                EncodeToMp3Windows(fileName);
        }

        private static void EncodeToMp3Windows(string fileName)
        {
            var oggReader = new VorbisWaveReader(fileName);
            MediaFoundationEncoder.EncodeToMp3(oggReader, fileName.Replace(".ogg", ".mp3"), 128000);
        }

        private static void ExecuteFFMPEG(string parameters)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $" -hide_banner -loglevel panic {parameters}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                // Console.WriteLine(line);
            }
        }

        private static void EncodeToMp3Linux(string filename)
        {
            var outputFile = Path.Combine(WorkingDirectory, Path.GetFileNameWithoutExtension(filename) + ".mp3");
            var parameters = $" -i {filename} -vn -n -ar 44100 -ac 2 -b:a 128k {outputFile}";

            ExecuteFFMPEG(parameters);
        }

        private static void DownloadFiles(List<Word> wordsList, string outputFolder, Func<string, string> formUrlFunctor)
        {
            WebClient webClient = new WebClient();

            foreach (var word in wordsList)
            {
                var outputFile = $"{outputFolder}/{word.English}.ogg";

                if (File.Exists(outputFile))
                    continue;

                var downloadUrl = formUrlFunctor(word.English);

                webClient.DownloadFile(downloadUrl, outputFile);
            }
        }

        private static void DownloadAudio(List<Word> wordsList)
        {
            WebClient webClient = new WebClient();

            var counter = 0;
            var total = wordsList.Count;

            foreach (var word in wordsList)
            {
                counter++;

                // Check if file exists
                var wordEn = word.English;
                var wordRu = word.Russian;

                if (File.Exists(Path.Combine(WorkingDirectory, $"{wordEn}.ogg")) &&
                    File.Exists(Path.Combine(WorkingDirectory, $"{wordRu}.ogg")))
                {
                    Console.WriteLine($"{counter}/{total} Skipping words pair '{wordEn}={wordRu}' as files are already downloaded");
                    word.HasAudio = true;
                    continue;
                }

                // First, download En word from En wiki and Oxford
                var urlOggEn = GetOxfordUrl(wordEn);
                if (string.IsNullOrEmpty(urlOggEn))
                    urlOggEn = GetWikiUrl(webClient, UrlEnWikiBaseHtml, wordEn);

                if (string.IsNullOrEmpty(urlOggEn))
                    Console.WriteLine("---> Not Found");

                // Then download Ru word from En and Ru wiki
                var urlOggRu = GetWikiUrl(webClient, UrlEnWikiBaseHtml, wordRu);
                if (string.IsNullOrEmpty(urlOggRu))
                    urlOggRu = GetWikiUrl(webClient, UrlRuWikiBaseHtml, wordRu);

                if (string.IsNullOrEmpty(urlOggRu) || string.IsNullOrEmpty(urlOggEn))
                {
                    Console.WriteLine($"{counter}/{total} [!] Skipping words pair '{wordEn}'='{wordRu}' due to missing audio files");
                    continue;
                }

                Console.WriteLine($"{counter}/{total} Downloading words pair '{wordEn}'='{wordRu}'");

                webClient.DownloadFile(urlOggEn, Path.Combine(WorkingDirectory, $"{wordEn}.ogg"));
                webClient.DownloadFile(urlOggRu, Path.Combine(WorkingDirectory, $"{wordRu}.ogg"));
                word.HasAudio = true;
            }
        }

        private static string GetWikiUrl(WebClient webClient, string baseUrl, string word)
        {
            string urlOgg = null;

            try
            {
                var html = webClient.DownloadString($"{baseUrl}/{word}");
                var matches = Regex.Match(html, "<source src=\"//upload.wikimedia.org/wikipedia/commons.+\\.ogg\" type");

                html = matches.Value;

                if (html.Contains("type=\"audio/mpeg\""))
                {
                    var r = Regex.Match(html, "<source src=.+<source src=");
                    html = html.Remove(0, r.Length - 12);
                }

                urlOgg = html.Replace("<source src=\"", "https:").Replace("\" type", string.Empty);
            }
            catch (Exception)
            {
                // Ignore exceptions
            }

            return urlOgg;
        }

        private static string GetOxfordUrl(string word)
        {
            // todo: remove workaround
            if (word == "condition" || word == "contain")
                word = $"x{word}";

            var paddedWord = word.PadRight(5, '_');
            var part1 = paddedWord.Substring(0, 1);
            var part2 = paddedWord.Substring(0, 3);
            var part3 = paddedWord.Substring(0, 5);

            var result = $"{UlrEnBase}/{part1}/{part2}/{part3}/{word}{UrlEnPostfix}";

            // todo: remove workaround
            if (word == "act" || word == "cap")
                result = result.Replace("us_1.ogg", "us_2.ogg");

            return result;
        }

        private static List<Word> ReadFilesListToDownload(string fileName)
        {
            var result = new List<Word>();

            var content = File.ReadAllLines(fileName);

            foreach (string line in content)
            {
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length == 2)
                {
                    var word = new Word();
                    word.English = keyvalue[0].Trim();
                    word.Russian = keyvalue[1].Trim();

                    result.Add(word);
                }
            }

            return result;
        }

        static void MergeOgg(string[] inputFiles, string outputFile)
        {
            // MediaFoundationInterop.MFStartup();

            var vorbisReader = new VorbisWaveReader("bias__us_1.ogg");

            MediaFoundationEncoder.EncodeToMp3(vorbisReader, outputFile);

            var outputStream = new FileStream("output.mp3", FileMode.Create);


            byte[] array = new byte[255];
            var span = new Span<byte>(array);
            vorbisReader.Read(span);

            outputStream.Write(span);
            //Mp3Frame frame;
            //while ((frame = reader.ReadNextFrame()) != null)
            //{
            //    outputStream.Write(frame.RawData, 0, frame.RawData.Length);
            //}
        }

        public static void Combine(string[] inputFiles, string outputFile)
        {
            var outputStream = new FileStream(outputFile, FileMode.Create);

            foreach (string file in inputFiles)
            {
                // new WaveProvider
                // MediaFoundationEncoder.EncodeToMp3(file,);



                Mp3FileReader reader = new Mp3FileReader(file);
                if ((outputStream.Position == 0) && (reader.Id3v2Tag != null))
                {
                    outputStream.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    outputStream.Write(frame.RawData, 0, frame.RawData.Length);
                }
            }
        }
    }
}
