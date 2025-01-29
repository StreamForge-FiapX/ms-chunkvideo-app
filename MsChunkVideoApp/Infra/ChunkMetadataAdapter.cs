using Dapper;
using Domain.Entities;
using Domain.Gateway;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infra
{
    public class ChunkMetadataAdapter : IChunkMetadataPort
    {
        private string _connectionString;

        public ChunkMetadataAdapter(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void SaveChunk(Chunk chunk)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("chunkname", chunk.ChunkName);
                parameters.Add("videoName", chunk.VideoName);
                parameters.Add("destinationBucket", chunk.DestinationBucket);

                string sqlCommand = "INSERT INTO chunk(chunkname, videoName, destinationBucket) VALUES(@chunkname, @videoName, @destinationBucket)";

                connection.Execute(sqlCommand, parameters);
            }
        }

    }
}
