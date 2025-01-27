using Dapper;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Infra
{
    public class VideoAdapter : IVideoPort
    {
        private string _connectionString;

        public VideoAdapter(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void SaveChunk(Chunk chunk)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("name", chunk.Id);
                parameters.Add("duration", chunk.Duration);

                string sqlCommand = "INSERT INTO chunk(name, duration) VALUES(@name, @duration)";

                connection.Execute(sqlCommand, parameters);
            }
        }

    }
}
