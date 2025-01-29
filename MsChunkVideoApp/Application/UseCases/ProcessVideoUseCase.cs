using Domain.Entities;
using Domain.Gateway;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Xabe.FFmpeg;

namespace Application.UseCases
{
    public class ProcessVideoUseCase(
        IChunkMetadataPort videoPort,
        IMessageQueuePort queuePort,
        IStoragePort storagePort,
        IVideoProcessorPort videoProcessorPort
            , IConfiguration configuration
            ) : IProcessVideoUseCase
    {
        private readonly IChunkMetadataPort _videoPort = videoPort;
        private readonly IMessageQueuePort _queuePort = queuePort;
        private readonly IStoragePort _storagePort = storagePort;
        private readonly IVideoProcessorPort _videoProcessorPort = videoProcessorPort;

        private string _downloadPath = configuration["FilePath:DownloadPath"];
        private string _chunkPath = configuration["FilePath:ChunkPath"];
        private string _destinationBucketName = configuration["AWS:S3:DestinationBucketName"];

        public async Task Process()
        {

            string videoName = _queuePort.ConsumeMessage();

            if (string.IsNullOrEmpty(videoName))
            {
                return;
            }

            string inputFilePath = Path.Combine(_downloadPath, $"{videoName}");
            await _storagePort.DownloadVideoAsync(videoName, inputFilePath);

            var chunks = await SplitIntoChunks(videoName, inputFilePath);

            foreach (var chunk in chunks)
            {
                await _storagePort.UploadChunkAsync(chunk.ChunkName, chunk.TempFolderPath);

                _videoPort.SaveChunk(chunk);

                string jsonChunk = JsonSerializer.Serialize(chunk);

                _queuePort.PublishMessage(jsonChunk);

                File.Delete(chunk.TempFolderPath);
            }

            File.Delete(inputFilePath);
        }

        public async Task<IEnumerable<Chunk>> SplitIntoChunks(string videoId, string inputFile)
        {
            var chunks = new List<Chunk>();

            Directory.CreateDirectory(_chunkPath);
            IMediaInfo mediaInfo = await _videoProcessorPort.GetMediaInfo(inputFile);
            TimeSpan duration = mediaInfo.Duration;

            int partNumber = 1;
            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan partDuration = TimeSpan.FromMinutes(1);


            string extension = videoId.Split('.')[1];
            string nameWithoutExtension = videoId.Split(".")[0];

            while (startTime < duration)
            {
                string chunkName = $"{nameWithoutExtension}_part{partNumber}.{extension}";
                string outputFilePath = Path.Combine(_chunkPath, chunkName);

                await _videoProcessorPort.SaveFile(inputFile, startTime, partDuration, outputFilePath);

                startTime = startTime.Add(partDuration);
                partNumber++;

                var chunk = new Chunk()
                {
                    ChunkName = chunkName,
                    VideoName = videoId,
                    DestinationBucket = _destinationBucketName,
                    TempFolderPath = outputFilePath
                };

                chunks.Add(chunk);

            }

            return chunks;
        }



    }
}
