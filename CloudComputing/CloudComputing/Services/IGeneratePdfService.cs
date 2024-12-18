namespace CloudComputing.Services
{
    public interface IGeneratePdfService
    {
        public Task<byte[]> GeneratePdfAsync(string redactedText);
    }
}
