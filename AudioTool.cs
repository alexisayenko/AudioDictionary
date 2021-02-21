using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace AudioDictionary
{
    internal static class AudioTool
    {
        public static void ConvertDownloadedAudio(WordPairList wordsList)
        {
            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                ConvertOggToMp3(word.Article1);
                ConvertOggToMp3(word.Article2);

                word.HasAudio = ConvertOggToMp3(word.Word1);
                word.HasAudio &= ConvertOggToMp3(word.Word2);
            }
        }

        private static void MergeFileWindows(string fileName, FileStream outputStream)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

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

        private static bool ConvertOggToMp3(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;

            if (Environment.IsMp3Exist(word))
                return true;

            try
            {
                EncodeToMp3(Environment.GetWorkingPathToOgg(word));
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"(!) No Suitable Encoder for {Path.GetFileNameWithoutExtension(fileName)}");
                Console.WriteLine($"Exception on word '{word}'.");
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        private static void EncodeToMp3(string fileName)
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

        private static void EncodeToMp3Linux(string filename)
        {
            var outputFile = Environment.GetWorkingPathToMp3(Path.GetFileNameWithoutExtension(filename));
            var parameters = $" -i {filename} -vn -n -ar 44100 -ac 2 -b:a 128k {outputFile}";

            ExecuteFFMPEG(parameters);
        }

        [Obsolete]
        private static void MergeOgg(string[] inputFiles, string outputFile)
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

        private static void MergeFilesLinux(WordPairList wordsList, string outputMp3File, string pattern)
        {
            var lines = new List<string>();
            Console.WriteLine($"Audio Pattern is {pattern}");
            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                foreach (var ch in pattern)
                {
                    if (ch == '2')
                        lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word2)}.mp3'");
                    else if (ch == '1')
                        lines.Add($"file '{Environment.GetWorkingPathToFile(word.Word1)}.mp3'");
                    else if (ch == '.')
                        lines.Add($"file '{Environment.WorkingPathToSilenceFile}'");
                }
            }

            var textFile = Environment.GetWorkingPathToFile("mylist.txt");

            File.WriteAllLines(textFile, lines);

            Thread.Sleep(1000);

            ExecuteFFMPEG($" -y -f concat -safe 0 -i {textFile} -c copy {Environment.GetWorkingPathToFile(outputMp3File)}");
        }

        private static void MergeFilesWindows(WordPairList wordsList, string outputMp3File, string pattern)
        {
            var outputStream = new FileStream(Environment.GetWorkingPathToFile(outputMp3File), FileMode.Create);

            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                foreach (var ch in pattern)
                {
                    string fileName;
                    switch (ch)
                    {
                        case '.':
                            fileName = Environment.WorkingPathToSilenceFile;
                            MergeFileWindows(fileName, outputStream);
                            break;
                        case '1':
                            fileName = Environment.GetWorkingPathToMp3(word.Article1);
                            MergeFileWindows(fileName, outputStream);
                            fileName = Environment.GetWorkingPathToMp3(word.Word1);
                            MergeFileWindows(fileName, outputStream);
                            break;
                        case '2':
                            fileName = Environment.GetWorkingPathToMp3(word.Article2);
                            MergeFileWindows(fileName, outputStream);
                            fileName = Environment.GetWorkingPathToMp3(word.Word2);
                            MergeFileWindows(fileName, outputStream);
                            break;
                    }
                }
            }
        }

        public static void MergeFiles(WordPairList wordsList, string outputMp3File, string pattern)
        {
            if (Environment.IsLinux)
                MergeFilesLinux(wordsList, outputMp3File, pattern);
            else
                MergeFilesWindows(wordsList, outputMp3File, pattern);
        }

        public static void NormalizeAudio(WordPairList wordsList)
        {
            // Trim empy sound
            // Normalize Volume
            // Speed/Tempo?
        }
    }
}
