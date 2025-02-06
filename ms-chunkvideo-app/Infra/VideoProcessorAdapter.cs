using Domain.Gateway;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

namespace Infra
{
    public class VideoProcessorAdapter : IVideoProcessorPort
    {
        public VideoProcessorAdapter(IConfiguration configuration) {
            
            FFmpeg.SetExecutablesPath(configuration["FFmpeg:ExecutablePath"]);
        }

        public async Task SaveFile(string inputFile, TimeSpan startTime, TimeSpan partDuration, string outputFilePath)
        {
            await FFmpeg.Conversions.New()
                .AddParameter($"-i {inputFile} -ss {startTime} -t {partDuration} -c copy {outputFilePath}")
                .Start();
        }

        public async Task<IMediaInfo> GetMediaInfo(string inputFile)
        {
            return await FFmpeg.GetMediaInfo(inputFile);
        }


    }
}
