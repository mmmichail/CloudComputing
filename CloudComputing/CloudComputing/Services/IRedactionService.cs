namespace CloudComputing.Services
{
    public interface IRedactionService
    {
        public string RedactSensitiveInformation(string text);
    }
}
