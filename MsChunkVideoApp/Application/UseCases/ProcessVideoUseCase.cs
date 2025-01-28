using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Xabe.FFmpeg;

namespace Application.UseCases
{
    public class ProcessVideoUseCase(
        IVideoPort videoPort,
        IQueuePort queuePort,
        IStoragePort storagePort
            , IConfiguration configuration
            ) : IProcessVideoUseCase
    {
        private readonly IVideoPort _videoPort = videoPort;
        private readonly IQueuePort _queuePort = queuePort;
        private readonly IStoragePort _storagePort = storagePort;

        private string _downloadPath = configuration["FilePath:DownloadPath"];
        private string _chunkPath = configuration["FilePath:ChunkPath"];
        private string _sourceBucket = configuration["AWS:S3:SourceBucketName"];
        private string _destinationBucket = configuration["AWS:S3:DestinationBucketName"];

        public async void Process()
        {

            try
            {
                string videoId = _queuePort.ConsumeMessage();

                string inputFilePath = Path.Combine(_downloadPath, $"{videoId}");
                await _storagePort.DownloadVideoAsync(videoId, inputFilePath);

                var chunks = await SplitIntoChunks(videoId, inputFilePath);

                foreach (var chunk in chunks)
                {
                    await _storagePort.UploadChunkAsync(chunk.Id, _chunkPath);

                    _videoPort.SaveChunk(chunk);

                    string jsonChunk = JsonSerializer.Serialize(chunk);

                    _queuePort.PublishMessage(jsonChunk);

                    File.Delete(chunk.TempFolderPath);
                }

                File.Delete(inputFilePath);
            }
            catch (Exception)
            {
                throw;
            }            
        }


        private async Task<IEnumerable<Chunk>> SplitIntoChunks(string videoId, string inputFile)
        {
            FFmpeg.SetExecutablesPath(@"C:\FFmpeg\bin");
            var chunks = new List<Chunk>();

            Directory.CreateDirectory(_chunkPath);

            var mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
            TimeSpan duration = mediaInfo.Duration;

            int partNumber = 1;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan partDuration = TimeSpan.FromMinutes(1);

            try
            {

                while (startTime < duration)
                {
                    string chunkName = $"{videoId}_part{partNumber}.mp4";
                    string outputFilePath = Path.Combine(_chunkPath, chunkName);

                    await FFmpeg.Conversions.New()
                        .AddParameter($"-i {inputFile} -ss {startTime} -t {partDuration} -c copy {outputFilePath}")
                        .Start();

                    startTime = startTime.Add(partDuration);
                    partNumber++;

                    var chunk = new Chunk()
                    {
                        Id = chunkName,
                        VideoId = videoId,
                        Duration = partDuration,
                        TempFolderPath = outputFilePath
                    };

                    chunks.Add(chunk);

                }
            }
            catch (Exception ex)
            {
                throw new ProcessingVideoException(ex.Message);
            }

            return chunks;

        }
    }
}
