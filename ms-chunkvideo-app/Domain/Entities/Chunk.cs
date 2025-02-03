namespace Domain.Entities
{
    public class Chunk
    {
        public string ChunkName { get; set; }
        public string VideoName { get; set; }
        public string DestinationBucket { get; set; }
        public string TempFolderPath { get; set; }
    }
}
