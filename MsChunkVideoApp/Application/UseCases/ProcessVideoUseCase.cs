using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using Xabe.FFmpeg;

namespace Application.UseCases
{
    public class ProcessVideoUseCase : IProcessVideoUseCase
    {
        private readonly IVideoPort _videoPort;
        private readonly IQueuePort _queuePort;
        private readonly IStoragePort _storagePort;

        private string _downloadPath;
        private string _chunkPath;

        public ProcessVideoUseCase(
            IVideoPort videoPort,
            IQueuePort queuePort,
            IStoragePort storagePort
            ,IConfiguration configuration
            )
        {
            _videoPort = videoPort;
            _queuePort = queuePort;
            _storagePort = storagePort;

            _downloadPath = configuration["FilePath:DownloadPath"];
            _chunkPath = configuration["FilePath:ChunkPath"];
        }

        public async void Process()
        {

            try
            {
                //var chunk1 = new Chunk()
                //{

                //    Id = "teste.mp4",
                //    Duration = new TimeSpan(0,1,0),
                //};
                //_videoGateway.SaveChunk(chunk1);

                string videoId = _queuePort.ConsumeMessage();

                string inputFilePath = Path.Combine(_downloadPath, $"{videoId}.mp4");
                await SaveVideo(videoId, inputFilePath);

                var chunks = await SplitIntoChunks(videoId, inputFilePath);

                foreach (var chunk in chunks)
                {
                    //await _storageGateway.UploadChunkAsync("destination-bucket", chunk.Data);

                    _videoPort.SaveChunk(chunk);

                    string jsonChunk = JsonSerializer.Serialize(chunk);

                    _queuePort.PublishMessage(jsonChunk);

                    File.Delete(chunk.TempFolderPath);
                }

                File.Delete(inputFilePath);
            }
            catch (Exception ex)
            {
                throw;
            }            
        }

        private async Task SaveVideo(string videoId, string filePath)
        {
            var videoStream = await _storagePort.DownloadVideoAsync("source-bucket", videoId);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await videoStream.CopyToAsync(fileStream);
            }
        }

        private async Task<IEnumerable<Chunk>> SplitIntoChunks(string videoId, string inputFile)
        {
            FFmpeg.SetExecutablesPath(@"C:\FFmpeg\bin");
            var chunks = new List<Chunk>();

            Directory.CreateDirectory(_chunkPath);

            // Get the duration of the video
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

                    // Split the video part
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
