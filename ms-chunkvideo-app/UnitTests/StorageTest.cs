using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class StorageTest
    {
        private readonly IAmazonS3 _s3Client;

        [Fact]
        public async void DownloadVideoAsync()
        {
            var credentials = new BasicAWSCredentials("test", "test");

            string bucketName = "source-bucket-localstack";
            string filePath = @"D:\Temp\InputDir\videoplayback.mp4";
            string keyName = "videoplayback.mp4"; // Nome desejado para o arquivo no S3
            string localFilePath = @"D:\Temp\meuarquivo.mp4";


            var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = "http://localhost:4566",
                UseHttp = true,
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
            });

            try
            {

                var request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
                {
                    // Salve o arquivo no caminho local
                    await response.WriteResponseStreamToFileAsync(localFilePath, false, System.Threading.CancellationToken.None);
                    Console.WriteLine($"Arquivo '{keyName}' baixado com sucesso para {localFilePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao baixar o arquivo do S3: {ex.Message}");
            }

        }

        [Fact]
        public async void UploadChunkAsync()
        {

            //string filePath = @"D:\Temp\InputDir\videoplayback.mp4";

            //// Criando um Stream a partir do arquivo MP4
            //using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            //{
            //    var request = new PutObjectRequest
            //    {
            //        BucketName = "destination-bucket-localstack",
            //        Key = "test",
            //        InputStream = fs
            //    };

            //    await _s3Client.PutObjectAsync(request);
            //}
            var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");

            string bucketName = "uploaded-video-bucket";
            string filePath = @"D:\Temp\InputDir\videoplayback.mp4";
            string keyName = "videoplayback.mp4"; // Nome desejado para o arquivo no S3


            // Configuração do cliente S3
            //var s3Client = new AmazonS3Client(credentials: credentials, region: RegionEndpoint.USEast1, config: config); // Defina a região correta

            var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = "http://localhost:4566",
                UseHttp = true,
                ForcePathStyle = true,
                AuthenticationRegion = "us-east-1",
            });

            // Usando TransferUtility para facilitar o upload de arquivos grandes
            var fileTransferUtility = new TransferUtility(s3Client);

            try
            {
                // Enviando o arquivo para o bucket S3
                await fileTransferUtility.UploadAsync(filePath, bucketName, keyName);
                Console.WriteLine($"Arquivo enviado com sucesso para {bucketName}/{keyName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar o arquivo para o S3: {ex.Message}");
            }


        }
    }
}
