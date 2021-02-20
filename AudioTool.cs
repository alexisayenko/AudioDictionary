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
        public string Silence05sec => Environment.WorkingPathToSilenceFile;

        public void ConvertDownloadedAudio(WordPairList wordsList)
        {
            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                word.HasAudio = ConvertOggToMp3(Environment.GetWorkingPathToOgg(word.Word1));
                word.HasAudio &= ConvertOggToMp3(Environment.GetWorkingPathToOgg(word.Word2));
            }
        }

        private void MergeFilesWindows(WordPairList wordsList)
        {
            var outputStream = new FileStream(Environment.WorkingPathToResultMp3, FileMode.Create);

            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                var fileName = Environment.GetWorkingPathToMp3(word.Word2);
                MergeFileWindows(fileName, outputStream);
                MergeFileWindows(Silence05sec, outputStream);
                MergeFileWindows(Silence05sec, outputStream);

                fileName = Environment.GetWorkingPathToMp3(word.Word1);
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
            var outputFile = Environment.GetWorkingPathToMp3(Path.GetFileNameWithoutExtension(filename));
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

        private void MergeFilesLinux(WordPairList wordsList)
        {
            var lines = new List<string>();

            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word2)}.mp3'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word1)}.mp3'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word1)}.mp3'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word1)}.mp3'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word1)}.mp3'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word1)}.mp3'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
            }

            var textFile = Environment.GetWorkingPathToFile("mylist.txt");

            File.WriteAllLines(textFile, lines);

            Thread.Sleep(1000);

            ExecuteFFMPEG($" -y -f concat -safe 0 -i {textFile} -c copy {Environment.WorkingPathToResultMp3}");
        }

        public void MergeFiles(WordPairList wordsList)
        {
            if (Environment.IsLinux)
                MergeFilesLinux(wordsList);
            else
                MergeFilesWindows(wordsList);
        }

        public void NormalizeAudio(WordPairList wordsList)
        {
            // Trim empy sound
            // Normalize Volume
            // Speed/Tempo?
        }
    }
}
