using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace AudioDictionary
{
    class AudioTool
    {
        public string WorkingDirectory { get; private set; }
        public string Silence05sec { get; }

        public AudioTool(string workingDirectory, string silence05sec) 
        {
            WorkingDirectory = workingDirectory;
            Silence05sec = silence05sec;
        }

        public void ConvertDownloadedAudio(EnRuWordsList wordsList)
        {
            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                word.HasAudio = ConvertOggToMp3(Path.Combine(WorkingDirectory, $"{word.English}.ogg"));
                word.HasAudio &= ConvertOggToMp3(Path.Combine(WorkingDirectory, $"{word.Russian}.ogg"));
            }
        }


        private void MergeFilesWindows(EnRuWordsList wordsList, string outputFile)
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

        private bool ConvertOggToMp3(string fileName)
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

        private void EncodeToMp3(string fileName)
        {
            if (Environment.IsLinux)
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

        private void EncodeToMp3Linux(string filename)
        {
            var outputFile = Path.Combine(WorkingDirectory, Path.GetFileNameWithoutExtension(filename) + ".mp3");
            var parameters = $" -i {filename} -vn -n -ar 44100 -ac 2 -b:a 128k {outputFile}";

            ExecuteFFMPEG(parameters);
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

        private void MergeFilesLinux(EnRuWordsList wordsList, string outputFile)
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

        private void CopySilenceToWorkingDir()
        {
            var binPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var fullSilencePathFrom = Path.Combine(binPath, Silence05sec);
            var fullSilencePathTo = GetFullPath(Silence05sec);

            File.Copy(fullSilencePathFrom, fullSilencePathTo, true);
        }

        public void MergeFiles(EnRuWordsList wordsList, string outputFile)
        {
            if (Environment.IsLinux)
                MergeFilesLinux(wordsList, outputFile);
            else
                MergeFilesWindows(wordsList, outputFile);
        }

        private string GetFullPath(string fileName)
        {
            return Path.Combine(WorkingDirectory, fileName);
        }

        public void NormalizeAudio(EnRuWordsList wordsList)
        {
            // Trim empy sound
            // Normalize Volume
            // Speed/Tempo?
        }
    }
}
