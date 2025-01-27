using Domain.Entities;

namespace Domain.Repositories
{
    public interface IVideoPort
    {
        void SaveChunk(Chunk chunk);
    }
}
