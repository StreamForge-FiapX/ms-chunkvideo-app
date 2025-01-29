using Domain.Entities;
using Xabe.FFmpeg;

namespace Domain.Gateway
{
    public interface IVideoProcessorPort
    {
        Task SaveFile(string inputFile, TimeSpan startTime, TimeSpan partDuration, string outputFilePath);

        Task<IMediaInfo> GetMediaInfo(string inputFile);
    }
}
