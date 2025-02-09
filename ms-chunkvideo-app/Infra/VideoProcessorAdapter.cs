using Domain.Gateway;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;

namespace Infra
{
    public class VideoProcessorAdapter : IVideoProcessorPort
    {
        private readonly IFFmpegWrapper _ffmpegWrapper;

        public VideoProcessorAdapter(IConfiguration configuration, IFFmpegWrapper ffmpegWrapper) {
            
            FFmpeg.SetExecutablesPath(configuration["FFmpeg:ExecutablePath"]);
            _ffmpegWrapper = ffmpegWrapper;
        }

        public async Task SaveFile(string inputFile, TimeSpan startTime, TimeSpan partDuration, string outputFilePath)
        {
            await _ffmpegWrapper.StartConversion(inputFile, startTime, partDuration, outputFilePath);
        }

        public async Task<IMediaInfo> GetMediaInfo(string inputFile)
        {
            return await _ffmpegWrapper.GetMediaInfo(inputFile);
        }


    }
}
