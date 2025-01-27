namespace Domain.Entities
{
    public class Chunk
    {
        public string Id { get; set; }
        public string VideoId { get; set; }
        public TimeSpan Duration { get; set; }
        public string TempFolderPath { get; set; }
    }
}
