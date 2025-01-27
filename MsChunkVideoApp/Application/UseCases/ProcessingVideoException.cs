
namespace Application
{
    [Serializable]
    internal class ProcessingVideoException : Exception
    {
        public ProcessingVideoException()
        {
        }

        public ProcessingVideoException(string? message) : base(message)
        {
        }

        public ProcessingVideoException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}