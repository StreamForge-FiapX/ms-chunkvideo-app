using Xabe.FFmpeg;

public interface IFFmpegWrapper
{
    Task StartConversion(string inputFile, TimeSpan startTime, TimeSpan partDuration, string outputFilePath);
    Task<IMediaInfo> GetMediaInfo(string inputFile);
}

public class FFmpegWrapper : IFFmpegWrapper
{
    public async Task StartConversion(string inputFile, TimeSpan startTime, TimeSpan partDuration, string outputFilePath)
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