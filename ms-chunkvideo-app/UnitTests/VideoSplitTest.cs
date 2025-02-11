using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace UnitTests
{
    
    public class VideoSplitTest
    {

        [Fact]
        public async void Test1()
        {
            FFmpeg.SetExecutablesPath(@"C:\FFmpeg\bin");

            string inputFile = "D:\\Temp\\InputDir\\videoplayback.mp4";
            string outputDirectory = "D:\\Temp\\OutputDir";
            Directory.CreateDirectory(outputDirectory);

            // Get the duration of the video
            var mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
            TimeSpan duration = mediaInfo.Duration;

            int partNumber = 1;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan partDuration = TimeSpan.FromMinutes(1);

            while (startTime < duration)
            {
                string outputFilePath = Path.Combine(outputDirectory, $"output_part{partNumber}.mp4");

                // Split the video part
                await FFmpeg.Conversions.New()
                    .AddParameter($"-i {inputFile} -ss {startTime} -t {partDuration} -c copy {outputFilePath}")
                    .Start();

                startTime = startTime.Add(partDuration);
                partNumber++;
            }            
        }
    }
}
