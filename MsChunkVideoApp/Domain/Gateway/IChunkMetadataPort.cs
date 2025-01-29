using Domain.Entities;

namespace Domain.Gateway
{
    public interface IChunkMetadataPort
    {
        void SaveChunk(Chunk chunk);
    }
}
